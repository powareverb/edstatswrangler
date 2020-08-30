using EliteAPI;
using Somfic.Logging;

using System.IO;
using System.Threading;
using Somfic.Logging.Handlers;
using System;
using AutoMapper;
using EliteAPI.Events.Startup;
using EliteAPI.Events.Travel;
using System.Windows.Forms;

namespace EliteStatsWrangler
{
    internal class Program
    {
        private static EliteDangerousAPI EliteAPI;
        private static ActiveSessions currentSessions;
        private static bool _stopMiningOnFsdJump = true;
        private static bool _consoleDebug = true;
        private static bool _raiseOnCatchup = true;
        private static MapperConfiguration _mapperConfig;

        private static void Main(string[] args)
        {
            var userDir = System.Environment.GetEnvironmentVariable("USERPROFILE");
            var journalDirectoryPath = userDir + @"\Saved Games\Frontier Developments\Elite Dangerous";
            var journalDirectory = new DirectoryInfo(journalDirectoryPath);
            if(!journalDirectory.Exists)
            {
                Console.WriteLine("Could not find journal directory");
                Console.WriteLine($"Tried: {journalDirectoryPath}");
                Application.Exit();
            }
            EliteConfiguration cfg = new EliteConfiguration()
            {
                JournalDirectory = journalDirectory,
                RaiseOnCatchup = _raiseOnCatchup,
                UseDiscordRichPresence = false,
            };
            EliteAPI = new EliteDangerousAPI(cfg);
            if(_consoleDebug)
                Logger.AddHandler(new ConsoleHandler());

            _mapperConfig = new MapperConfiguration(mapcfg =>
                {
                    mapcfg.CreateMap<StatSession, StatSessionSummary>();
                }
            ); 

            currentSessions = new ActiveSessions(Directory.GetCurrentDirectory());
            currentSessions.ObjectMapper = _mapperConfig.CreateMapper();

            EliteAPI.Events.LoadGameEvent += Events_LoadGameEvent;
            EliteAPI.Events.ShutdownEvent += Events_ShutdownEvent;
            EliteAPI.Events.CommanderEvent += Events_CommanderEvent;
            EliteAPI.Events.LoadoutEvent += Events_LoadoutEvent;
            EliteAPI.Events.StatusInMainMenu += Events_StatusInMainMenu;
            EliteAPI.Events.MusicEvent += Events_MusicEvent;

            // Mining
            EliteAPI.Events.CargoEvent += Events_CargoEvent;
            EliteAPI.Events.ProspectedAsteroidEvent += Events_ProspectedAsteroidEvent;
            EliteAPI.Events.LaunchDroneEvent += Events_LaunchDroneEvent;
            EliteAPI.Events.MiningRefinedEvent += Events_MiningRefinedEvent;
            EliteAPI.Events.AsteroidCrackedEvent += Events_AsteroidCrackedEvent;

            // Trade related
            EliteAPI.Events.MarketSellEvent += Events_MarketSellEvent;
            EliteAPI.Events.MarketBuyEvent += Events_MarketBuyEvent;

            // Travel related
            EliteAPI.Events.StatusFsdChargingEvent += Events_StatusFsdChargingEvent;
            EliteAPI.Events.FSDJumpEvent += Events_FSDJumpEvent;
            EliteAPI.Events.LocationEvent += Events_LocationEvent;
            EliteAPI.Events.SupercruiseEntryEvent += Events_SupercruiseEntryEvent;
            EliteAPI.Events.SupercruiseExitEvent += Events_SupercruiseExitEvent;
            EliteAPI.Events.DockedEvent += Events_DockedEvent;
            EliteAPI.Events.UndockedEvent += Events_UndockedEvent;

            // Exploration
            EliteAPI.Events.FuelScoopEvent += Events_FuelScoopEvent;
            EliteAPI.Events.SellExplorationDataEvent += Events_SellExplorationDataEvent;

            // Scavenging
            EliteAPI.Events.StatusScoopingEvent += Events_StatusScoopingEvent;

            // Mission running
            EliteAPI.Events.MissionAbandonedEvent += Events_MissionAbandonedEvent;
            EliteAPI.Events.MissionAcceptedEvent += Events_MissionAcceptedEvent;
            EliteAPI.Events.MissionCompletedEvent += Events_MissionCompletedEvent;
            EliteAPI.Events.MissionFailedEvent += Events_MissionFailedEvent;
            EliteAPI.Events.MissionRedirectedEvent += Events_MissionRedirectedEvent;
            EliteAPI.Events.MissionsEvent += Events_MissionsEvent;

            // Combat
            EliteAPI.Events.CockpitBreachedEvent += Events_CockpitBreachedEvent;
            EliteAPI.Events.RedeemVoucherEvent += Events_RedeemVoucherEvent;

            // Tricks
            EliteAPI.Events.AllEvent += Events_AllEvent;

            EliteAPI.Start();

            Console.WriteLine("Press any key to quit");
            Console.ReadKey();
        }

        private static void Events_RedeemVoucherEvent(object sender, EliteAPI.Events.RedeemVoucherInfo e)
        {
            // Add bounty vouchers
            if (e.Type.Equals("Bounty", StringComparison.OrdinalIgnoreCase))
            {
                var session = currentSessions.StartedCombat(e.Timestamp, EventReasons.RedeemedBountyVoucher);
                session.RedeemedBountyVoucher(e.Amount, e.Factions, currentSessions.CurrentLocation);
            }
            else if (e.Type.Equals("Combat", StringComparison.OrdinalIgnoreCase))
            {
                var session = currentSessions.StartedCombat(e.Timestamp, EventReasons.RedeemedCombatBonds);
                session.RedeemedBountyVoucher(e.Amount, e.Factions, currentSessions.CurrentLocation);
            } 
            else
            {
                // ?
            }

            // InfluencePeddling?
        }

        private static void Events_AllEvent(object sender, EliteAPI.Events.EventBase e)
        {
            if(e as EliteAPI.Events.ReceiveTextInfo != null)
            {
                var msg = e as EliteAPI.Events.ReceiveTextInfo;
                if(msg.Channel.Equals("npc"))
                {
                } 
                else
                {
                }
            }
            if (e as EliteAPI.Events.SendTextInfo != null)
            {
                var msg = e as EliteAPI.Events.SendTextInfo;
            }
        }

        private static void Events_MissionsEvent(object sender, MissionsInfo e)
        {
            //var session = currentSessions.StartedMissionRunning(e.Timestamp, EventReasons.CargoScooping);
        }

        private static void Events_MissionRedirectedEvent(object sender, EliteAPI.Events.MissionRedirectedInfo e)
        {
            var session = currentSessions.StartedMissionRunning(e.Timestamp, EventReasons.MissionRedirected);
        }

        private static void Events_MissionFailedEvent(object sender, EliteAPI.Events.MissionFailedInfo e)
        {
            var session = currentSessions.StartedMissionRunning(e.Timestamp, EventReasons.MissionFailed);
            var deets = new MissionDetails()
            {
                MissionId = e.MissionId,
                Timestamp = e.Timestamp,
            };
            session.FailedMission(deets);
        }

        private static void Events_MissionCompletedEvent(object sender, EliteAPI.Events.MissionCompletedInfo e)
        {
            var session = currentSessions.StartedMissionRunning(e.Timestamp, EventReasons.MissionCompleted);
            var deets = new MissionDetails()
            {
                MissionId = e.MissionId,
                Faction = e.Faction,
                TargetFaction = e.TargetFaction,
                //DestinationSystem = e.DestinationSystem,
                //DestinationStation = e.DestinationStation,
                FactionEffects = e.FactionEffects,
                MaterialsReward = e.MaterialsReward,
                Timestamp = e.Timestamp,
            };
            session.CompletedMission(deets);
        }

        private static void Events_MissionAcceptedEvent(object sender, EliteAPI.Events.MissionAcceptedInfo e)
        {
            var session = currentSessions.StartedMissionRunning(e.Timestamp, EventReasons.MissionAccepted);
            var deets = new MissionDetails()
            {
                MissionId = e.MissionId,
                Faction = e.Faction,
                Expiry = e.Expiry,
                //DestinationSystem = e.DestinationSystem,
                //DestinationStation = e.DestinationStation,
                PassengerCount = e.PassengerCount,
                CommodityName = e.CommodityLocalised,
                CommodityCount = e.Count,
                Timestamp = e.Timestamp,
                Influence = e.Influence,
                Reputation = e.Reputation,
            };
            session.AcceptedMission(deets);
        }

        private static void Events_MissionAbandonedEvent(object sender, EliteAPI.Events.MissionAbandonedInfo e)
        {
            var session = currentSessions.StartedMissionRunning(e.Timestamp, EventReasons.MissionAbandoned);
            var deets = new MissionDetails()
            {
                MissionId = e.MissionId,
                Timestamp = e.Timestamp,
            };
            session.AbandonedMission(deets);
        }

        private static void Events_CockpitBreachedEvent(object sender, EliteAPI.Events.CockpitBreachedInfo e)
        {
            var session = currentSessions.StartedCombat(e.Timestamp, EventReasons.CargoScooping);
            session.CockpitBreached();
        }

        private static void Events_StatusScoopingEvent(object sender, EliteAPI.Events.StatusEvent e)
        {
            var session = currentSessions.StartedScavenging(e.Timestamp, EventReasons.CargoScooping);
        }

        private static void Events_FuelScoopEvent(object sender, EliteAPI.Events.FuelScoopInfo e)
        {
            var session = currentSessions.StartedExploring(e.Timestamp, EventReasons.FuelScooping);
            session.ScoopedFuel(e.Scooped);
        }

        private static void Events_SellExplorationDataEvent(object sender, EliteAPI.Events.SellExplorationDataInfo e)
        {
            // Add to missions too?
            var session = currentSessions.StartedExploring(e.Timestamp.ToUniversalTime(), EventReasons.FuelScooping);
            session.SoldExplorationData(e, currentSessions.CurrentLocation);
            //session.ScoopedFuel(e.Scooped);
            // InfluencePeddling?
        }

        private static void Events_StatusFsdChargingEvent(object sender, EliteAPI.Events.StatusEvent e)
        {
            if (_stopMiningOnFsdJump)
                currentSessions.StoppedMining(e.Timestamp.ToUniversalTime(), EventReasons.FSDCharge);

            if (_stopMiningOnFsdJump)
                currentSessions.StoppedCombat(e.Timestamp.ToUniversalTime(), EventReasons.FSDCharge);

            // We will combine travel and trade in a summary
            if (_stopMiningOnFsdJump)
                currentSessions.StoppedTrading(e.Timestamp.ToUniversalTime(), EventReasons.FSDCharge);

            if (_stopMiningOnFsdJump)
                currentSessions.StoppedScavenging(e.Timestamp.ToUniversalTime(), EventReasons.FSDCharge);
        }

        private static void Events_MusicEvent(object sender, EliteAPI.Events.MusicInfo e)
        {
            if (e.MusicTrack.Equals("MainMenu", StringComparison.OrdinalIgnoreCase))
                currentSessions.Shutdown(e.Timestamp);
        }

        private static void Events_StatusInMainMenu(object sender, EliteAPI.Events.StatusEvent e)
        {
            if (e.Event.Equals("Status.InMainMenu", StringComparison.OrdinalIgnoreCase) && e.Value.Equals(true))
                currentSessions.Shutdown(e.Timestamp);
        }

        private static void Events_DockedEvent(object sender, DockedInfo e)
        {
            var tmp = new CommanderTravelLocation()
            {
                 SystemName = e.StarSystem,
                 SystemAddress = e.SystemAddress,
                 BodyName = e.StationName,
                 BodyType = "Station",
                 MarketId = e.MarketId
            };
            currentSessions.UpdatedLocation(e.Timestamp, tmp, EventReasons.Docked);
            currentSessions.StoppedTravelling(e.Timestamp, EventReasons.Docked);

            if (_stopMiningOnFsdJump)
                currentSessions.StoppedCombat(e.Timestamp, EventReasons.Docked);
            if (_stopMiningOnFsdJump)
                currentSessions.StoppedExploring(e.Timestamp, EventReasons.Docked);
            if (_stopMiningOnFsdJump)
                currentSessions.StoppedMining(e.Timestamp, EventReasons.Docked);
            if (_stopMiningOnFsdJump)
                currentSessions.StoppedScavenging(e.Timestamp, EventReasons.Docked);

        }

        private static void Events_UndockedEvent(object sender, EliteAPI.Events.UndockedInfo e)
        {
            currentSessions.StartedTravelling(e.Timestamp, EventReasons.Undocked);
            var tmp = new CommanderTravelLocation()
            {
                SystemName = currentSessions.CurrentLocation.SystemName,
                SystemAddress = currentSessions.CurrentLocation.SystemAddress,
                BodyName = e.StationName,
                BodyType = "Station",
                MarketId = e.MarketId
            };
            currentSessions.UpdatedLocation(e.Timestamp, tmp, EventReasons.Undocked);

            if (_stopMiningOnFsdJump)
                currentSessions.StoppedTrading(e.Timestamp, EventReasons.Undocked);
        }

        private static void Events_MarketBuyEvent(object sender, EliteAPI.Events.MarketBuyInfo e)
        {
            var tradeSession = currentSessions.StartedTrading(e.Timestamp, EventReasons.BoughtGoods);
            tradeSession.UpdatedTrade(e.Timestamp, e.MarketId, "Buy", e.TypeLocalised, e.Count, e.BuyPrice);
        }

        private static void Events_MarketSellEvent(object sender, EliteAPI.Events.MarketSellInfo e)
        {
            var tradeSession = currentSessions.StartedTrading(e.Timestamp, EventReasons.SoldGoods);
            if (e.BlackMarket)
            {
                Console.WriteLine("Sold some stole goods lol");
            }
            if (e.StolenGoods)
            {
                Console.WriteLine("Sold some stole goods lol");
            }
            tradeSession.UpdatedTrade(e.Timestamp, e.MarketId, "Sell", e.TypeLocalised, e.Count, e.SellPrice);
        }

        private static void Events_CargoEvent(object sender, CargoInfo e)
        {
            Console.WriteLine($"Got cargo event: {e.Event}");
            try
            {
                if (e.Inventory != null)
                {
                    foreach (var inv in e.Inventory)
                    {
                        Console.WriteLine($"Inv: {inv.NameLocalised} cnt {inv.Count}");
                    }
                }
                else
                {
                    Console.WriteLine("Inventory error?");
                }
            }
            catch (Exception ex)
            {
                // Bail...
            }
        }

        private static void Events_FSDJumpEvent(object sender, FSDJumpInfo e)
        {
            var reason = EventReasons.FSDJump;
            var tmp = new CommanderTravelLocation()
            {
                SystemName = e.StarSystem,
                SystemAddress = e.SystemAddress,
                BodyName = e.Body,
                BodyType = "Star",
                MarketId = null
            };
            currentSessions.UpdatedLocation(e.Timestamp, tmp, reason);
            if(!currentSessions.IsActivelyTravelling)
            {
                //currentSessions.StartedTravelling(e.Timestamp);
            }

            if (_stopMiningOnFsdJump)
                currentSessions.StoppedMining(e.Timestamp, reason);

            if (_stopMiningOnFsdJump)
                currentSessions.StoppedCombat(e.Timestamp, reason);

            // We will combine travel and trade in a summary
            if (_stopMiningOnFsdJump)
                currentSessions.StoppedTrading(e.Timestamp, reason);

        }

        private static void Events_LoadoutEvent(object sender, LoadoutInfo e)
        {
            // TODO: Dump ship loadout for comparisons?
            if(e.ShipName != currentSessions.CurrentShip)
            {
                // Swapped ship
                Console.WriteLine($"Swapped from ship:  {currentSessions.CurrentShip} ID: {currentSessions.CurrentShipIdent}");
                Console.WriteLine($"Swapped to ship:    {e.ShipName} ID: {e.ShipIdent}");
                currentSessions.CurrentShip = e.ShipName;
                currentSessions.CurrentShipIdent = e.ShipIdent;
            }
        }

        private static void Events_CommanderEvent(object sender, CommanderInfo e)
        {
            currentSessions.CommanderName = e.Name;
        }

        private static void Events_ShutdownEvent(object sender, EliteAPI.Events.ShutdownInfo e)
        {
            currentSessions.Shutdown(e.Timestamp);
        }

        private static void Events_SupercruiseExitEvent(object sender, EliteAPI.Events.SupercruiseExitInfo e)
        {
            var tmp = new CommanderTravelLocation()
            {
                SystemName = e.StarSystem,
                SystemAddress = e.SystemAddress,
                BodyName = e.Body,
                BodyType = e.BodyType,
                MarketId = null
            };
            currentSessions.UpdatedLocation(e.Timestamp, tmp, EventReasons.ExitedSupercruise);
        }

        private static void Events_SupercruiseEntryEvent(object sender, EliteAPI.Events.SupercruiseEntryInfo e)
        {
            var tmp = new CommanderTravelLocation()
            {
                SystemName = e.StarSystem,
                SystemAddress = e.SystemAddress,
                BodyName = currentSessions.CurrentLocation.BodyName,
                BodyType = currentSessions.CurrentLocation.BodyType,
                MarketId = null,
            };
            currentSessions.UpdatedLocation(e.Timestamp, tmp, EventReasons.EnteredSupercruise);
            // We're travelling now
            currentSessions.StartedTravelling(e.Timestamp, EventReasons.EnteredSupercruise);

            if (_stopMiningOnFsdJump)
                currentSessions.StoppedMining(e.Timestamp, EventReasons.EnteredSupercruise);

            if (_stopMiningOnFsdJump)
                currentSessions.StoppedCombat(e.Timestamp, EventReasons.EnteredSupercruise);

            // We will combine travel and trade in a summary
            if (_stopMiningOnFsdJump)
                currentSessions.StoppedTrading(e.Timestamp, EventReasons.EnteredSupercruise);
        }

        private static void Events_LocationEvent(object sender, EliteAPI.Events.LocationInfo e)
        {
            var reason = EventReasons.ChangedLocation;
            var tmp = new CommanderTravelLocation()
            {
                SystemName = e.StarSystem,
                SystemAddress = e.SystemAddress,
                BodyName = e.Body,
                BodyType = e.BodyType,
                MarketId = null,
            };
            currentSessions.UpdatedLocation(e.Timestamp.UtcDateTime, tmp, reason);
            // Should we start travelling?
            if(!currentSessions.IsActivelyTravelling)
            {
                //currentSessions.StartedTravelling(e.Timestamp);
            }
        }

        private static void Events_LoadGameEvent(object sender, EliteAPI.Events.LoadGameInfo e)
        {
            currentSessions.CurrentShip = e.ShipName;
            currentSessions.CommanderName = e.Commander;
            currentSessions.CurrentShipIdent = e.ShipIdent;

            var reason = EventReasons.LoadedGame;
            currentSessions.StoppedMining(e.Timestamp, reason);
            currentSessions.StoppedCombat(e.Timestamp, reason);
            currentSessions.StoppedTrading(e.Timestamp, reason);
            currentSessions.StoppedTravelling(e.Timestamp, reason);
        }


        private static void Events_MiningRefinedEvent(object sender, EliteAPI.Events.MiningRefinedInfo e)
        {
            var miningSession = currentSessions.StartedMining(e.Timestamp, EventReasons.RefinedMinerals);
            miningSession.AddRefinedMinerals(e.Timestamp, e.TypeLocalised);
        }

        private static void Events_AsteroidCrackedEvent(object sender, EliteAPI.Events.AsteroidCrackedInfo e)
        {
            var miningSession = currentSessions.StartedMining(e.Timestamp, EventReasons.CrackedAsteroid);
            miningSession.AddCrackedAsteroid(e.Timestamp, e.Event);
        }


        private static void Events_LaunchDroneEvent(object sender, EliteAPI.Events.LaunchDroneInfo e)
        {
            // Updating mining session
            if (e.Type.Equals("Prospector"))
            {
                var session = currentSessions.StartedMining(e.Timestamp, EventReasons.LaunchedProspector);
                session.AddProspectorLaunched(e.Timestamp);
            }
            else if (e.Type.Equals("Collection"))
            {
                if(currentSessions.IsActivelyMining)
                {
                    var session = currentSessions.GetMiningSession(e.Timestamp, EventReasons.LaunchedCollector);
                    session.AddCollectorLaunched(e.Timestamp);
                }
                else
                {
                    // Scavenging?
                }
            }
        }

        private static void Events_ProspectedAsteroidEvent(object sender, EliteAPI.Events.ProspectedAsteroidInfo e)
        {
            var session = currentSessions.GetMiningSession(e.Timestamp.UtcDateTime, EventReasons.ProspectedAsteroid);
            session.AddProspectedAsteroid(e.MotherlodeMaterial, e.ContentLocalised, e.Materials);
        }
    }
}