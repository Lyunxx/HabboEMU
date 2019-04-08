using Plus.Database.Manager.Database.Session_Details.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;

namespace Plus.HabboHotel.Pets
{
    /// <summary>
    /// Class PetCommandHandler.
    /// </summary>
    internal class PetCommandHandler
    {
        /// <summary>
        /// The _table
        /// </summary>
        private static DataTable _table;

        /// <summary>
        /// The _pet commands
        /// </summary>
        private static Dictionary<string, PetCommand> _petCommands;

        /// <summary>
        /// Gets the pet commands.
        /// </summary>
        /// <param name="pet">The pet.</param>
        /// <returns>Dictionary&lt;System.Int16, System.Boolean&gt;.</returns>
        internal static Dictionary<short, bool> GetPetCommands(Pet pet)
        {
            var output = new Dictionary<short, bool>();
            var qLevel = (short)pet.Level;

            switch (pet.Type)
            {
                default:
                    {
                        output.Add(0, true); // FREE
                        output.Add(7, true); // FOLLOW
                    }
                    break;
            }

            return output;
        }

        /// <summary>
        /// Initializes the specified database client.
        /// </summary>
        /// <param name="dbClient">The database client.</param>
        internal static void Init(IQueryAdapter dbClient)
        {
            dbClient.SetQuery("SELECT * FROM pets_commands");
            _table = dbClient.GetTable();
            _petCommands = new Dictionary<string, PetCommand>();
            foreach (DataRow row in _table.Rows)
            {
                _petCommands.Add(row[1].ToString(), new PetCommand(Convert.ToInt32(row[0].ToString()), row[1].ToString()));
            }
        }

        /// <summary>
        /// Tries the invoke.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>System.Int32.</returns>
        internal static int TryInvoke(string input)
        {
            PetCommand command;
            return _petCommands.TryGetValue(input, out command) ? command.CommandId : 0;
        }
    }
}