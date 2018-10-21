using System;
using LM = LogsStorage.LogsManagerInterface;
using DB = DatabaseHandler.DatabaseConnectionInterface;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace BiStorage
{
    class Sessions : BaseEventHandler
    {
        public Sessions(LM lm, DB db) : base(lm, db) {}

        public override bool process(string jsonMessage)
        {
            this.logger().logMessage(string.Format("Handling message in {0} event handler", this.GetType().Name));
            dynamic message = JsonConvert.DeserializeObject<dynamic>(jsonMessage);

//message.session_id = this.getRandom(32); //for dev testing

            // if we already have this session id we can ignore the message
            List<Dictionary<string, string>> sessions = this.db().fetch(
                "SELECT Id FROM Sessions WHERE Id = @sessionId",
                new Dictionary<string, string>() {
                    {"sessionId", message.session_id.ToString()}
                }
            );
            if (sessions.Count > 0) {
                this.logger().logMessage(string.Format(
                    "Record with same sesion id already exists - ignoring the message in {0} event handler",
                    this.GetType().Name
                ));

                return true;
            }

            // since afffilaite id is a foreign key ensure related record exists in affiliates table
            int insertedAffiliates = this.db().execute(
                @"
                    MERGE INTO Affiliates AS target
                    USING (
                        SELECT @id AS Id, @default AS Name, @default AS Location, @default AS Notes, @creationDate AS CreationDate
                    ) AS source
                    ON (target.Id = source.Id)
                    WHEN MATCHED THEN UPDATE SET target.Id = target.Id
                    WHEN NOT MATCHED THEN INSERT (Id, Name, Location, Notes, CreationDate)
                    VALUES (@id, @default, @default, @default, @creationDate);
                ",
                new Dictionary<string, string>() {
                    {"id", message.affiliate_id.ToString()},
                    {"default", this.getDefault()},
                    {"creationDate", DateTimeOffset.Now.ToString()}
                }
            );
            if (insertedAffiliates != 1) {
                this.logger().logMessage(string.Format(
                    "Could not ensure existence of affiliates record for affiliate id {0} in {1} event handler",
                    message.affiliate_id.ToString(),
                    this.GetType().Name
                ));

                return false;
            }

            // insert session data
            int insertedSessions = this.db().execute(
                @"
                    INSERT INTO Sessions (id, AffiliateId, CampaignId, Ip, SiteName, CreationDate)
                    VALUES (@sessionId, @affiliateId, @campaignId, @ip, @siteName, @creationDate)
                ",
                new Dictionary<string, string>() {
                    {"sessionId", message.session_id.ToString()},
                    {"affiliateId", message.affiliate_id.ToString()},
                    {"campaignId", message.campaign_id.ToString()},
                    {"ip", message.ip.ToString()},
                    {"siteName", message.site_name.ToString()},
                    {"creationDate", message.creation_date.ToString()}
                }
            );
            if (insertedSessions != 1) {
                this.logger().logMessage(string.Format(
                    "Could not insert session record for session id {0} in {1} event handler",
                    message.session_id.ToString(),
                    this.GetType().Name
                ));

                return false;

            }
            this.logger().logMessage(string.Format(
                "Message processed for session id {0} in {1} event handler",
                message.session_id.ToString(),
                this.GetType().Name
            ));

            return true;
        }
    }
}
