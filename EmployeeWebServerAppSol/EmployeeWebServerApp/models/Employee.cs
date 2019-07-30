using System;
using System.Collections;
using System.Collections.Generic;
using BasicWebServerLib;

namespace EmployeeWebServerApp.models {
  public class Employee {
    private readonly Helpers _helpers;
    public Employee() {
      _helpers = new Helpers();
    }
    public int Id {get;set;}
    public string Name {get;set;}
    public int Age {get;set;}
    public string Address {get;set;}
    public bool Active {get;set;}
    public DateTime Hired {get;set;}
    public ArrayList Family {get;set;}
    public string Picture {get;set;}
    
    public Dictionary<string, object> EmployeeToDictionary() {
      Dictionary<string, object> dict = new Dictionary<string, object>() {
        {"Id", Id},
        {"Name", Name},
        {"Age", Age},
        {"Address", Address},
        {"Active", Active},
        {"Hired", Hired},
        {"Family", Family},
        {"picture", Picture}
      };
      return dict;
    }

    public void DictionaryToEmployee(Dictionary<string, object> dict) {
      foreach(KeyValuePair<string, object> entry in dict) {
        switch(entry.Key) {
          case "name":
            Name = (string)dict[entry.Key];
            break;
          case "age":
            Age = Convert.ToInt32(dict[entry.Key]);
            break;
          case "address":
            Address = (string)dict[entry.Key];
            break;
          case "active":
            Active = (bool)dict[entry.Key];
            break;
          case "hired":
            string[] dateParts = ((string)dict[entry.Key]).Split('-');
            DateTime hired = new DateTime(Int32.Parse(dateParts[0]),Int32.Parse(dateParts[1]),Int32.Parse(dateParts[2]));
            Hired = hired;
            break;
          case "family":
            Family = _helpers.JArrayToArrayList(dict[entry.Key]);
            break;
          case "picture":
            Picture = (string)dict[entry.Key];
            break;
        }
      }
    }
  }
}