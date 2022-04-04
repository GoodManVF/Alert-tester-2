using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;
using cAlgo.API.Alert;

using cAlgo.API.Alert.Utility;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public class AlertTester2 : Robot
    {
        [Parameter("WMA Slow ", Group = "INDICATOR CONTROLS", DefaultValue = "200", MinValue = "50", MaxValue = "500")]
        public int WmaPeriodSlow { get; set; }

        [Parameter("WMA Fast ", Group = "INDICATOR CONTROLS", DefaultValue = "60", MinValue = "3", MaxValue = "500")]
        public int WmaPeriodFast { get; set; }

        [Parameter("CCI Period ", Group = "INDICATOR CONTROLS", DefaultValue = "20", MinValue = "10", MaxValue = "50")]
        public int CciPeriod { get; set; }

        [Parameter("ADX Period ", Group = "INDICATOR CONTROLS", DefaultValue = "14", MinValue = "10", MaxValue = "50")]
        public int ADXPeriod { get; set; }

        [Parameter("ADX enter level ", Group = "INDICATOR CONTROLS", DefaultValue = "25", MinValue = "10", MaxValue = "50")]
        public int ADXLevel { get; set; }

        [Parameter("Min Distance from ma Breaks", Group = "DISTANCES CONTROL", DefaultValue = "25000")]
        public int DistanceBreak { get; set; }

        [Parameter("Max Distance from ma Breaks", Group = "DISTANCES CONTROL", DefaultValue = "60000")]
        public int MaxDistanceBreak { get; set; }

        [Parameter("Max Distance from ma Bounces", Group = "DISTANCES CONTROL", DefaultValue = "20000")]
        public int DistanceToBounce { get; set; }

        //--------------------------------------------------------------------------------------------------------//

        private WeightedMovingAverage sma;
        private WeightedMovingAverage fsma;
        private CommodityChannelIndex cci;
        private AverageDirectionalMovementIndexRating adx;

        //--------------------------------------------------------------------------------------------------------//

        public double Parameter { get; set; }
        private int _barIndex;
        double UpCrossPrice;
        double DownCrossPrice;
        bool CanMakeAnotherBuy = false;
        bool CanMakeAnotherSell = false;

        //------------------------------------------------------------------------------------------------------//
        protected override void OnStart()
        {
            Notifications.ShowPopup(Bars.TimeFrame, Symbol, "Started");
            //Notifications.ShowPopup(Bars.TimeFrame, Symbol, TradeType.Buy, "Alert Test 2 cBot", Symbol.Bid, "BUY..", Server.Time);
            sma = Indicators.WeightedMovingAverage(Bars.ClosePrices, WmaPeriodSlow);
            fsma = Indicators.WeightedMovingAverage(Bars.ClosePrices, WmaPeriodFast);
            cci = Indicators.CommodityChannelIndex(CciPeriod);
            adx = Indicators.AverageDirectionalMovementIndexRating(ADXPeriod);
            Logger.Tracer = Print;
        }
        protected override void OnBar()
        {
            int index = Bars.ClosePrices.Count - 1;
            /*if (_barIndex != index)
            {
                _barIndex = index;

                TradeType tradeType = Bars.ClosePrices[index - 1] > Bars.OpenPrices[index - 1] ? TradeType.Buy : TradeType.Sell;

                Notifications.ShowPopup(Bars.TimeFrame, Symbol, tradeType, "Alert Test 2 cBot", Symbol.Bid, "Just Testing..", Server.Time);
                //Notifications.TriggerAlert(Symbol.Bid,);
            }*/

            //---------------------------------------------------------------------------------------------------------------------------------
            double distanceToMaPriceBuyBreak = (sma.Result.LastValue + DistanceBreak * Symbol.PipSize);
            double distanceToMaPriceSellBreak = (sma.Result.LastValue - DistanceBreak * Symbol.PipSize);
            //---------------------------------------------------------------------------------------------------------------------------------
            if (Bars.ClosePrices.HasCrossedAbove(sma.Result.LastValue, 1))
            {
                CanMakeAnotherBuy = true;
                CanMakeAnotherSell = false;
                double _upCrossPrice = sma.Result.LastValue;
                UpCrossPrice = _upCrossPrice;
            }
            if (UpCrossPrice != 0 && (Bars.ClosePrices.LastValue - UpCrossPrice) / Symbol.PipSize > MaxDistanceBreak)
            {
                CanMakeAnotherBuy = false;
                UpCrossPrice = 0;
            }
            if (Bars.ClosePrices.HasCrossedBelow(sma.Result.LastValue, 1))
            {
                CanMakeAnotherBuy = false;
                CanMakeAnotherSell = true;
                double _downCrossPrice = sma.Result.LastValue;
                DownCrossPrice = _downCrossPrice;
            }

            if (DownCrossPrice != 0 && (DownCrossPrice - Bars.ClosePrices.LastValue) / Symbol.PipSize > MaxDistanceBreak)
            {
                CanMakeAnotherSell = false;
                DownCrossPrice = 0;
            }
            //----------------------------------------------------------------------------------------------------------------------------------
            if (CanMakeAnotherBuy && Bars.ClosePrices.LastValue > distanceToMaPriceBuyBreak && adx.ADX.LastValue > ADXLevel)
            {

                Print("Opening buy");
                Notifications.ShowPopup(Bars.TimeFrame, Symbol, TradeType.Buy, "Alert Test 2 cBot", Symbol.Bid, "BUY..", Server.Time);
                CanMakeAnotherBuy = false;
            }

            if (CanMakeAnotherSell && Bars.ClosePrices.LastValue < distanceToMaPriceSellBreak && adx.ADX.LastValue > ADXLevel)
            {

                Print("Opening sell");
                Notifications.ShowPopup(Bars.TimeFrame, Symbol, TradeType.Sell, "Alert Test 2 cBot", Symbol.Bid, "SELL..", Server.Time);
                CanMakeAnotherSell = false;
            }
        }
        protected override void OnTick()
        {

        }

        protected override void OnStop()
        {
            Notifications.ShowPopup(Bars.TimeFrame, Symbol, "Stopped");
        }
    }
}


