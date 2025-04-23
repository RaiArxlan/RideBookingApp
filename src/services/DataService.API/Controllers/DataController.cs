using DataService.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace DataService.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class DataController : ControllerBase
    {
        private readonly ILogger<DataController> _logger;
        private readonly IConfiguration _configuration;

        public DataController(ILogger<DataController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

            // Check if table exists
            try
            {
                var connectionStringWithoutDatabase = GetConnectionStringWithoutDatabase();
                var databaseName = GetDatabaseName();

                // Ensure database exists
                using (var connection = new NpgsqlConnection(connectionStringWithoutDatabase))
                {
                    connection.Open();
                    _logger.LogInformation("Connection to server opened successfully.");

                    using (var command = new NpgsqlCommand($"SELECT 1 FROM pg_database WHERE datname = '{databaseName}'", connection))
                    {
                        var exists = command.ExecuteScalar() != null;
                        _logger.LogInformation("Checked existence of database '{DatabaseName}': Exists = {Exists}", databaseName, exists);

                        if (!exists)
                        {
                            _logger.LogInformation("Database '{DatabaseName}' does not exist. Creating database...", databaseName);
                            using (var createCommand = new NpgsqlCommand($"CREATE DATABASE \"{databaseName}\"", connection))
                            {
                                createCommand.ExecuteNonQuery();
                                _logger.LogInformation("Database '{DatabaseName}' created successfully.", databaseName);
                            }
                        }
                    }
                }

                // Ensure table exists
                using (var connection = new NpgsqlConnection(GetConnectionString()))
                {
                    connection.Open();
                    _logger.LogInformation("Database connection opened successfully.");

                    using (var command = new NpgsqlCommand("SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'person')", connection))
                    {
                        var exists = (bool)command.ExecuteScalar();
                        _logger.LogInformation("Checked existence of 'person' table: Exists = {Exists}", exists);

                        if (!exists)
                        {
                            _logger.LogInformation("'person' table does not exist. Creating table...");
                            using (var createCommand = new NpgsqlCommand("CREATE TABLE person (Id SERIAL PRIMARY KEY, Name VARCHAR(100), Email VARCHAR(100), Phone VARCHAR(15), Occupation VARCHAR(100), Address VARCHAR(255))", connection))
                            {
                                createCommand.ExecuteNonQuery();
                                _logger.LogInformation("'person' table created successfully.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while ensuring the database and 'person' table exist: {Message}", ex.Message);
            }
        }

        // GET : Establish connectivity with Postgres and fetch all object, and return as json list, barebone, old fashion, no ORM
        [HttpGet]
        public IEnumerable<Person> Get()
        {
            NpgsqlConnection connection = null;
            try
            {
                _logger.LogInformation("Request received : {Controller}->{Action}", nameof(DataController), nameof(Get));

                // Connection
                var connectionString = this.GetConnectionString();
                connection = new NpgsqlConnection(connectionString);
                connection.Open();

                // Read
                using var command = new NpgsqlCommand("SELECT * FROM person", connection);
                using var reader = command.ExecuteReader();
                var people = new List<Person>();

                while (reader.Read())
                {
                    var person = new Person
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        Name = reader.GetString(reader.GetOrdinal("Name")),
                        Email = reader.GetString(reader.GetOrdinal("Email")),
                        Phone = reader.GetString(reader.GetOrdinal("Phone")),
                        Occupaiton = reader.GetString(reader.GetOrdinal("Occupation")),
                        Address = reader.GetString(reader.GetOrdinal("Address"))
                    };
                    people.Add(person);
                }

                // Return data
                return people;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching data: {Message}", ex.Message);
                return Enumerable.Empty<Person>();
            }
            finally
            {
                connection?.Dispose();
                _logger.LogInformation("Request completed : {Controller}->{Action}", nameof(DataController), nameof(Get));
            }
        }

        // GET : Establish connectivity with Postgres and fetch object by id, and return as json list, barebone, old fashion, no ORM
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            NpgsqlConnection connection = null;
            try
            {
                _logger.LogInformation("Request received : {Controller}->{Action}", nameof(DataController), nameof(Get));
                // Connection
                var connectionString = this.GetConnectionString();
                connection = new NpgsqlConnection(connectionString);
                connection.Open();
                // Read
                using var command = new NpgsqlCommand("SELECT * FROM person WHERE Id = @Id", connection);
                command.Parameters.AddWithValue("Id", id);
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    var person = new Person
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        Name = reader.GetString(reader.GetOrdinal("Name")),
                        Email = reader.GetString(reader.GetOrdinal("Email")),
                        Phone = reader.GetString(reader.GetOrdinal("Phone")),
                        Occupaiton = reader.GetString(reader.GetOrdinal("Occupation")),
                        Address = reader.GetString(reader.GetOrdinal("Address"))
                    };
                    return Ok(person);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching data: {Message}", ex.Message);
                return StatusCode(500, "Internal server error");
            }
            finally
            {
                connection?.Dispose();
                _logger.LogInformation("Request completed : {Controller}->{Action}", nameof(DataController), nameof(Get));
            }
        }

        // POST : Establish connectivity with Postgres and insert object, and return as json list, barebone, old fashion, no ORM
        [HttpPost]
        public IActionResult Post([FromBody] Person person)
        {
            NpgsqlConnection connection = null;
            try
            {
                _logger.LogInformation("Request received : {Controller}->{Action}", nameof(DataController), nameof(Post));
                // Connection
                var connectionString = this.GetConnectionString();
                connection = new NpgsqlConnection(connectionString);
                connection.Open();
                // Insert
                using var command = new NpgsqlCommand("INSERT INTO person (Name, Email, Phone, Occupation, Address) VALUES (@Name, @Email, @Phone, @Occupation, @Address) RETURNING Id", connection);
                command.Parameters.AddWithValue("Name", person.Name);
                command.Parameters.AddWithValue("Email", person.Email);
                command.Parameters.AddWithValue("Phone", person.Phone);
                command.Parameters.AddWithValue("Occupation", person.Occupaiton);
                command.Parameters.AddWithValue("Address", person.Address);
                var newId = (int)command.ExecuteScalar();
                person.Id = newId;
                return Ok(person);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while inserting data: {Message}", ex.Message);
                return StatusCode(500, "Internal server error");
            }
            finally
            {
                connection?.Dispose();
                _logger.LogInformation("Request completed : {Controller}->{Action}", nameof(DataController), nameof(Post));
            }
        }

        private string GetConnectionString()
        {
            var isDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
            var connectionString = isDocker
                ? this._configuration.GetConnectionString("DockerConnectionString")
                : this._configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                _logger.LogError("Connection string is not set.");
            }

            _logger.LogInformation("Using connection string: {ConnectionString}", connectionString);

            return connectionString;
        }

        private string GetConnectionStringWithoutDatabase()
        {
            var connectionString = GetConnectionString();
            var builder = new NpgsqlConnectionStringBuilder(connectionString)
            {
                Database = null // Remove the database part
            };
            return builder.ToString();
        }

        private string GetDatabaseName()
        {
            var connectionString = GetConnectionString();
            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            return builder.Database;
        }

    }
}
