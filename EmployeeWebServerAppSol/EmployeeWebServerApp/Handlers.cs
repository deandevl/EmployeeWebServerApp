

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using BasicWebServerLib;
using BasicWebServerLib.WsCommon;
using BasicWebServerLib.Events;
using BasicWebServerLib.HttpCommon;
using EmployeeWebServerApp.models;
using LiteDB;

namespace EmployeeWebServerApp {
  public class Handlers {
    private readonly string _serverBaseFolder;
    private readonly Helpers _helpers;
    private readonly Dictionary<string, Action> _actions;
    private readonly LiteCollection<Employee> _employeeCol;
    private Dictionary<string, object> _requestDictionary;
    private HttpConnectionDetails _httpDetails;
    
    public Handlers(string serverBaseFolder, string databasePath, string collectionName) {
      _serverBaseFolder = serverBaseFolder;
      _helpers = new Helpers();
      LiteDatabase db = new LiteDatabase(databasePath);
      _employeeCol = db.GetCollection<Employee>(collectionName);
      
      //create database if necessary
      if(!File.Exists(databasePath)) { 
        AddEmployee(
          "Bob Jones", 47,"107 Spalding Ave, Columbus,Ohio", true, new DateTime(1997, 11, 3),
          new ArrayList(){"Sally", "Jim"}, "bobjones.jpeg");
        
        AddEmployee(
          "Sam Peters", 35,"543 Casper Street, Dayton,Ohio", true, new DateTime(2004, 1, 13),
          new ArrayList(){"Martha"}, "sampeters.jpeg");
        
        AddEmployee(
          "Jane Stewart",53, "121 Title Ave, Cleveland,Ohio", true, new DateTime(2016, 7, 3),
          new ArrayList(){"Jim","Peter","Bobby"}, "janestewart.jpeg");
        
        AddEmployee(
          "Sally Smith", 27,"27 Canal Street, Columbus,Ohio", true, new DateTime(2010, 6, 23),
          new ArrayList(){"James", "Tommy","Nancy"}, "sallysmith.jpeg");
      } 
      _employeeCol.EnsureIndex(x => x.Name);
      
      _actions = new Dictionary<string, Action>() {
        {"add", () => {
          int id = AddEmployee();
          string name = (string)_requestDictionary["name"];
          string response = "Added " + name + " with id " + id + " successfully.";
          _helpers.SendHttpTextResponse(_httpDetails.Response,response);
        }},
        {"get",() => {
          Dictionary<string, object> response = GetEmployee();
          _helpers.SendHttpJsonResponse(_httpDetails.Response,response);
        }},
        {"update", () => {
          bool updated = UpdateEmployee();
          string name = (string)_requestDictionary["name"];
          _helpers.SendHttpTextResponse(_httpDetails.Response,name + " updated is " + updated);
        }},
        {"delete", () => {
          string name = (string)_requestDictionary["name"];
          int count = DeleteEmployee();
          _helpers.SendHttpTextResponse(_httpDetails.Response, name + " deleted " + count + " employee successfully");
        }}, 
        {"all", () => {
            string response = GetAllEmployees();
            _helpers.SendHttpTextResponse(_httpDetails.Response,response);
          }},
        {"error500", () => {
          byte[] buffer = new byte[0];
          _helpers.SendHttpResponse(500, "Produced 500 Error: Internal Server Error",buffer,"text/html","EmployeeServer", _httpDetails.Response);
        }}
      };
    }

    public void StartServer() {
      BasicWebServer server = new BasicWebServer(baseFolderPath: _serverBaseFolder);
      server.WsTextFrameChanged += WsTextFrameChanged;
      server.HttpRequestChanged += HttpRequestChanged;
      
      server.Start();
    }
    private void WsTextFrameChanged(object sender, EventArgs args) {
      WsTextFrameEventArgs wsArgs = (WsTextFrameEventArgs) args;
      
      WsConnectionDetails details = wsArgs.Details; 
      string message = (string)wsArgs.Message;
      //string pathOrFileName = details.PathOrFileName; //debug
      NetworkStream stream = details.Stream;
      WsFrameWriter wsFrameWriter = new WsFrameWriter(stream);

      _requestDictionary = _helpers.JsonToDictionary(message);
      string name = (string) _requestDictionary["name"];
      if(details.PathOrFileName == "employee") {
        if((string) _requestDictionary["action"] == "add") {
          int id = AddEmployee();
          Dictionary<string,object> responseDict = new Dictionary<string, object>() {
            {"action","add"},
            {"message","Added " + name + " with id " + id + " successfully."}
          };
          _helpers.SendWsText(wsFrameWriter,responseDict);
        } else if((string) _requestDictionary["action"] == "get") {
          Dictionary<string, object> employeeJson = GetEmployee();
          Dictionary<string,object> responseDict = new Dictionary<string, object>() {
            {"action","get"},
            {"message",employeeJson}
          };
          _helpers.SendWsText(wsFrameWriter,responseDict);
        }
      }
    }

    private void HttpRequestChanged(object sender, EventArgs args) {
      HttpRequestEventArgs httpArgs = (HttpRequestEventArgs) args;
      _httpDetails = httpArgs.Details;
      string body = (string)httpArgs.Body;
      _requestDictionary = _helpers.JsonToDictionary(body);
      
      if(_httpDetails.HttpPath == "employee") {
        _actions[(string)_requestDictionary["action"]]();
      }
    }

    private int AddEmployee(string name, int age, string address, bool active, DateTime hired, ArrayList family,
      string picture){
      Employee employee = new Employee{
        Name = name,
        Age = age,
        Address = address,
        Active = active,
        Hired = hired,
        Family = family,
        Picture = picture
      };
      return _employeeCol.Insert(employee);
    }
    private int AddEmployee() {
      Employee employee = new Employee();
      employee.DictionaryToEmployee(_requestDictionary);
      employee.Id = _employeeCol.Insert(employee);
      return employee.Id;
    }

    private Dictionary<string, object> GetEmployee() {
      string name = (string)_requestDictionary["name"];
      Employee employee = _employeeCol.FindOne(x => x.Name.Equals(name));
      Dictionary<string, object> employeeDict = employee.EmployeeToDictionary();
      return employeeDict;
    }

    private bool UpdateEmployee() {
      string name = (string)_requestDictionary["name"];
      Employee employee = _employeeCol.FindOne(x => x.Name.Equals(name));
      employee.DictionaryToEmployee(_requestDictionary);
      bool updated = _employeeCol.Update(employee);
      return updated;
    }

    private int DeleteEmployee() {
      string name = (string)_requestDictionary["name"];
      int count = _employeeCol.Delete(x => x.Name.Equals(name));
      return count;
    }

    private string GetAllEmployees() {
      var employees = _employeeCol.FindAll();
      ArrayList names = new ArrayList();
      foreach(Employee employee in employees) {
        names.Add(employee.Name);
      }
      return _helpers.ArrayToJson(names);
    }
  }
}