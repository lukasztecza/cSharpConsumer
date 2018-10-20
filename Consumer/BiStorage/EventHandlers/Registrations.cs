using System;
using LM = LogsStorage.LogsManagerInterface;
using DB = DatabaseHandler.DatabaseConnectionInterface;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace BiStorage
{
    class Registrations : BaseEventHandler
    {
        public Registrations(LM lm, DB db) : base(lm, db) {}

        public override bool process(string jsonMessage)
        {
            this.logger().logMessage(string.Format("Handling message in {0} event handler", this.GetType().Name));
            dynamic message = JsonConvert.DeserializeObject<dynamic>(jsonMessage);

            // if we already have registration for this username at this site at that datetime we can ignore the message
            List<Dictionary<string, string>> registrations = this.db().fetch(
                @"
                    SELECT UserName FROM Registrations
                    WHERE UserName = @userName
                    AND SiteName = @siteName
                    AND CreationDate = @creationDate
                ",
                new Dictionary<string, string>() {
                    {"userName", message.user_name.ToString()},
                    {"siteName", message.site_name.ToString()},
                    {"creationDate", message.creation_date.ToString()}
                }
            );
            if (registrations.Count > 0) {
                this.logger().logMessage(string.Format(
                    "Registration with the same username, usersite and creation date already exists - ignoring the message in {0} event handler",
                    this.GetType().Name
                ));

                return true;
            }

            // find internal session id for passed session identifier
            List<Dictionary<string, string>> sessions = this.db().fetch(
                "SELECT Id FROM Sessions WHERE SessionId = @sessionId",
                new Dictionary<string, string>() {
                    {"sessionId", message.session_id.ToString()}
                }
            );
            if (sessions.Count != 1) {
                this.logger().logMessage(string.Format(
                    "Could not get session for session id {0} in {1} event handler",
                    message.session_id.ToString(),
                    this.GetType().Name
                ));

                return false;
            }

            // insert registration data
            int insertedRegistrations = this.db().execute(
                @"
                    INSERT INTO Registrations (UserId, UserName, Email, SiteName, SessionId, Ip, CreationDate)
                    VALUES (@userId, @userName, @email, @siteName, @sessionId, @ip, @creationDate)
                ",
                new Dictionary<string, string>() {
                    {"userId", message.user_id.ToString()},
                    {"userName", message.user_name.ToString()},
                    {"email", message.email.ToString()},
                    {"siteName", message.site_name.ToString()},
                    {"sessionId", sessions[0]["Id"]},
                    {"ip", message.ip.ToString()},
                    {"creationDate", message.creation_date.ToString()}
                }
            );
            if (insertedRegistrations != 1) {
                this.logger().logMessage(string.Format(
                    "Could not insert registration record for session id {0} in {1} event handler",
                    message.session_id.ToString(),
                    this.GetType().Name
                ));

                return false;

            }
            this.logger().logMessage(string.Format("Message processed in {0} event handler", this.GetType().Name));

            return true;
        }
    }
}
