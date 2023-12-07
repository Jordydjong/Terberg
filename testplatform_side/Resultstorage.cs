using System;
using System.IO.Ports;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using System.Dynamic;
using System.Net.Http.Headers;
using System.Threading.Tasks;


namespace storage{
    /// <summary>
    /// Class for collection, storage and sending of the testdata
    /// </summary>
    class Resultstorage{
        Tests_data tests_data;
        List<Tests_data> tests;
        int TcmId;
        string API_url;

        string username = "Bram";
        string password = "Wachtwoord";
        string login_url = $"https://hutcm.wkong.nl/auth/login";
        string bearer_token;
        /// <summary>
        /// Constructor function for the Resultstorage
        /// </summary>
        /// <param name="api_url">String that contains the url for the API where the data has to be sent</param>
        /// <param name="tcm_id">Integer that represents the id of the SUT</param>
        public Resultstorage(string api_url, int tcm_id){
            API_url = api_url;
            TcmId = tcm_id;
            tests = new List<Tests_data>();
        }
        /// <summary>
        /// Function to add the results of a test to the list of tests_data.
        /// </summary>
        /// <param name="name">String that contains the name of the test</param>
        /// <param name="description">String that contains the error code or a different message</param>
        /// <param name="succesfull">Boolean to indicate wether the test passed or failed</param>
        /// <returns>true if succeeded</returns>
        public bool add_testdata(string name, string description, bool succesfull){
            Tests_data tests_data= new Tests_data (name, description, succesfull);
            tests.Add(tests_data);
            return true;
        }

        public async Task<bool> get_bearertoken(){
            login_data data_login = new login_data(username, password);
            try{
                using var client = new HttpClient();
                var json = System.Text.Json.JsonSerializer.Serialize(data_login);
                Console.WriteLine(json);
                HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(login_url, content);
                if (response.IsSuccessStatusCode){
                    var result_api = await response.Content.ReadAsStringAsync();
                    var values = JsonSerializer.Deserialize<Dictionary<string, string>>(result_api);
                    Console.WriteLine(values["token"]);
                    bearer_token = values["token"];
                    return true;
                }else{
                    Console.WriteLine($"Failed with status code {response.StatusCode}");
                    return false;
                }
            }catch (Exception ex){
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Function that send a testreport to the API url in json format
        /// </summary>
        public async Task<bool> send_testreport(){
            await this.get_bearertoken();
            Test_information test_data = new Test_information(Guid.NewGuid(),tests);
            try{
                using var client = new HttpClient();
                var json = JsonSerializer.Serialize(test_data);
                HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.bearer_token);
                var response = await client.PostAsync(API_url, content);
                if (response.IsSuccessStatusCode){
                    var result_api = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(result_api);
                    return true;
                }else{
                    Console.WriteLine($"Failed with status code {response.StatusCode}");
                    return false;
                }
            }catch (Exception ex){
                Console.WriteLine(ex.ToString());
                return false;
            }
        }
    }
    /// <summary>
    /// Class that holds the list of tests and the SUT's id
    /// </summary>
    class Test_information{
        public Guid TcmId{get;set;}
        public List<Tests_data> Tests{get;set;}
        /// <summary>
        /// Constructor for the class
        /// </summary>
        /// <param name="Tcmid">Guid that represents the id</param>
        /// <param name="Tests">List from Tests_data that represents all the performed tests</param>
        public Test_information(Guid Tcmid, List<Tests_data> Tests){
            this.TcmId = Tcmid;
            this.Tests = Tests;
        }
    }

    /// <summary>
    /// Class that holds the testdata, it contains a name, a description and a boolean representing pass of fail
    /// </summary>
    class Tests_data{
        public string Name{get;set;}
        public string Description{get;set;}
        public bool Successfull{get;set;}

        public Tests_data(string name, string description, bool successfull){
            this.Name = name;
            this.Description = description;
            this.Successfull = successfull;
        }
    }

    class login_data{
        public string Username{get;set;}
        public string Password{get;set;}
        public login_data(string username, string password){
            this.Username = username;
            this.Password = password;
        }
    }

    class response_data{
        public string id{get;set;}
        public string username{get;set;}
        public string token{get;set;}
    }
};
