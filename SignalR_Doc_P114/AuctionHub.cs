using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace SignalR_Doc_P114
{
    public class AuctionHub : Hub
    {
        public AuctionHub()
        {
            BidManager.Start();
        }

        public override Task OnConnected()
        {
            Clients.Caller.CloseBid();
            Clients.All.UpdateBid(BidManager.CurrentBid);
            return base.OnConnected();
        }

        public void MakeCurrentBid()
        {
            BidManager.CurrentBid.BidPrice += 1;
            BidManager.CurrentBid.ConnectionId = this.Context.ConnectionId;
            Clients.All.UpdateBid(BidManager.CurrentBid);
        }

        public void MakeBid(double bid)
        {
            if(bid < BidManager.CurrentBid.BidPrice)
            {
                return;
            }
            BidManager.CurrentBid.BidPrice = bid;
            BidManager.CurrentBid.ConnectionId = this.Context.ConnectionId;
            Clients.All.UpdateBid(BidManager.CurrentBid);
        }
    }

    public static class BidManager
    {
        static Timer _timer = new Timer(BidInterval, null, 0, 2000);
        public static Bid CurrentBid { get; set; }

        public static void Start()
        {

        }
        public static void BidInterval(object o)
        {
            var client = GlobalHost.ConnectionManager.GetHubContext<AuctionHub>().Clients;
            if(BidManager.CurrentBid == null || BidManager.CurrentBid.TimeLeft <= 0)
            {
                BidManager.SetBid();
            }
            BidManager.CurrentBid.TimeLeft -= 2;
            if(BidManager.CurrentBid.TimeLeft <= 0)
            {
                client.AllExcept(CurrentBid.ConnectionId).CloseBid();
                if (!string.IsNullOrWhiteSpace(CurrentBid.ConnectionId))
                    client.Client(CurrentBid.ConnectionId).CloseBidWin(CurrentBid);
            }
        }
        static List<Bid> _items = new List<Bid>()
        {
            new Bid(){Name="Laptop", Description = "Best Laptop in the world", TimeLeft = 30, BidPrice = 250.0},
            new Bid(){Name="Iphone", Description = "Iphone of Obama", TimeLeft = 30, BidPrice = 1750.0},
            new Bid(){Name="Computer", Description = "The first computer in the world", TimeLeft = 30, BidPrice = 7000.0},
            new Bid(){Name="Car", Description = "This is the standard for production of Lamborgini", TimeLeft = 30, BidPrice = 15000.0},
            new Bid(){Name="Headphone", Description = "The sound is very good", TimeLeft = 30, BidPrice = 490.0},
        };
        public static void SetBid()
        {
            Random rnd = new Random();
            CurrentBid = (Bid)_items[rnd.Next(0, _items.Count - 1)].Clone();
        }
    }

    public class Bid
    {
        public Bid Clone()
        {
            return (Bid)MemberwiseClone();
        }
        public string Name { get; set; }
        public string Description { get; set; }
        public double BidPrice { get; set; }
        public int TimeLeft { get; set; }
        public string ConnectionId { get; set; }
    }
}