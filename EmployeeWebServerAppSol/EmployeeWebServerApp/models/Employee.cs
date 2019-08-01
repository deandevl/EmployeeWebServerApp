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
  }
}