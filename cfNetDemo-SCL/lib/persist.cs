using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;


namespace cfNetDemo.lib
{
    public class persist
    {
        public static NpgsqlConnection client; //Postgre client

        public persist()
        {
            Connect();
            

        }
        public void Initialize()
        {
            Console.WriteLine("Initializing DB");

            //var query = "CREATE TABLE IF NOT EXISTS \"items\"(\"code\" varchar(256) NOT NULL, \"name\" varchar(256) NOT NULL, \"integrated\" boolean NOT NULL)";
            var query = "CREATE TABLE IF NOT EXISTS items (code varchar(256) NOT NULL, name varchar(256) NOT NULL, integrated boolean NOT NULL)";
            var createTableCmd = new NpgsqlCommand(query, client);
            client.Open();
            createTableCmd.ExecuteNonQuery();
            client.Close();

            Console.WriteLine("DB Initialized");
        }

        public static List<ItemDB> Select()
        {
            Console.WriteLine("Select DB records");

            var query = "SELECT code, name, integrated FROM items where integrated = false";
            var SelectTableCmd = new NpgsqlCommand(query, client);
            client.Open();
            NpgsqlDataReader dr = SelectTableCmd.ExecuteReader();
            Console.WriteLine("Recrods Selected");


            List<ItemDB> rows = new List<ItemDB>();
            ItemDB row;

            // Output rows 
            while (dr.Read())
            {
                Console.WriteLine("{0} \t {1}", dr[0], dr[1]);
                row = new ItemDB(dr[0].ToString(), dr[1].ToString(), Convert.ToBoolean(dr[2]));
                rows.Add(row); 
            }
            client.Close();
            return rows;
        }

        public static void Insert(string body)
        {
           /** Implement Item Insertion **/
        }

        public void Update(string item)
        {
            /** Implement Item Update **/

        }

        public void Connect()
        {
            Console.WriteLine("Retriving Cloud Foundry Services");
            string dbServiceName = Environment.GetEnvironmentVariable("DB_SERVICE_NAME"); //postgresql
            string vcapServices =  Environment.GetEnvironmentVariable("VCAP_SERVICES");
            Console.WriteLine(vcapServices);
            
            
            // if DB is bound to the App on Cloud Foundry
            if (vcapServices != null)
            {
                dynamic json = JsonConvert.DeserializeObject(vcapServices);
                foreach (dynamic obj in json.Children())
                {
                    if (((string)obj.Name).ToLowerInvariant().Contains(dbServiceName))
                    {
                        dynamic credentials = (((JProperty)obj).Value[0] as dynamic).credentials;

                        //.Net connects to Postgre using a connection string and not the PG uri
                        String connectionString = "User ID=" + credentials.username;
                        connectionString += ";Password=" + credentials.password;
                        connectionString += ";Host=" + credentials.hostname;
                        connectionString += ";Port=" + credentials.port;
                        connectionString += ";Database=" + credentials.dbname;
                        connectionString += ";Pooling=true;";
                        Console.WriteLine("Connecting to DB on: " + connectionString);
                        client = new NpgsqlConnection(connectionString);
                        Console.WriteLine("Connected to the DB!");
                        Initialize();
                    }
                }
            }
            else
            {
                Console.WriteLine("Connecting to Local DB!");
                client = new NpgsqlConnection("User ID=postgres;Password=a1b2c3;Host=localhost;Port=5432;Database=itemdb;Pooling=true;");
                Console.WriteLine("Connected to the DB!");
                Initialize();
            }

        }
        public void Disconnect()
        {

        }
    }
}
