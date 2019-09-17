using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uniconta.Common;
using Uniconta.DataModel;
using Uniconta.ClientTools.DataModel;
using Uniconta.API.System;

namespace Uniconta.API.DebtorCreditor
{
    public class FindPrices
    {
        public Uniconta.DataModel.DCAccount debtor;
        DCOrder order;
        InvPriceListLine[] CustomerPrices;
        QueryAPI api;
        SQLCache items, priceLists;
        public bool UseCustomerPrices;
        InvPristListLineSort priceSort;
        byte PriceListCurrency, CompCurrency, OrderCurrency;
        public double ExchangeRate;
        double MarginRatio, SalesCharge;
        bool RoundMargin;
        string _PriceList;
        byte PriceGrp;
        Task LoadingTask;
        DateTime OrderTime;
        public Task<double> ExchangeTask;
        static double LastExchangeRate;
        static byte LastOrderCurrency;

        public FindPrices(DCOrder order, QueryAPI api)
        {
            this.order = order;
            this.api = api;
            LoadingTask = LoadBaseData(api);
        }

        public FindPrices(QueryAPI api)
        {
            this.api = api;
        }

        public Task OrderChanged(DCOrder order)
        {
            this.order = order;
            return LoadingTask = LoadBaseData(api);
        }

        public Task DebitorChanged()
        {
            return LoadingTask = LoadBaseData(api);
        }
         public Task1 DebitorChanged()
        {
            return LoadingTask = LoadBaseData(api);
        }

        private async Task LoadBaseData(QueryAPI api)
        {
            var Comp = api.CompanyEntity;
            this.CompCurrency = (byte)Comp._CurrencyId;
            this.CustomerPrices = null;

            if (order == null)
                return;

            Type MasterType, GroupType, PriceListType;
            if (order.__DCType() != 2)
            {
                MasterType = typeof(Uniconta.DataModel.Debtor);
                GroupType = typeof(Uniconta.DataModel.DebtorGroup);
                PriceListType = typeof(Uniconta.DataModel.DebtorPriceList);
                UseCustomerPrices = Comp.InvPrice;
            }
            else
            {
                MasterType = typeof(Uniconta.DataModel.Creditor);
                GroupType = typeof(Uniconta.DataModel.CreditorGroup);
                PriceListType = typeof(Uniconta.DataModel.CreditorPriceList);
                UseCustomerPrices = Comp.CreditorPrice;
            }

            OrderCurrency = (byte)order._Currency;
            if (OrderCurrency == 0)
                OrderCurrency = CompCurrency;
            if (OrderCurrency != CompCurrency)
            {
                if (LastOrderCurrency == OrderCurrency)
                    this.ExchangeRate = LastExchangeRate;
                this.ExchangeTask = api.session.ExchangeRate((Currencies)CompCurrency, (Currencies)OrderCurrency, DateTime.Now, Comp);
                this.ExchangeRate = await this.ExchangeTask.ConfigureAwait(false);
                LastExchangeRate = this.ExchangeRate;
                LastOrderCurrency = OrderCurrency;
                this.ExchangeTask = null;
            }

            this.items = Comp.GetCache(typeof(Uniconta.DataModel.InvItem)) ?? await Comp.LoadCache(typeof(Uniconta.DataModel.InvItem), api).ConfigureAwait(false);

            if (order._DCAccount != null)
            {
                var Debs = Comp.GetCache(MasterType) ?? await Comp.LoadCache(MasterType, api).ConfigureAwait(false);

                var debtor = (Uniconta.DataModel.DCAccount)Debs.Get(order._DCAccount);
                if (debtor != null)
                {
                    PriceGrp = Common.Utility.Util.Value(debtor._PriceGroup);
                    this.debtor = debtor;
                    this._PriceList = order._PriceList ?? debtor._PriceList;
                    if (this._PriceList == null)
                    {
                        var DebGrps = Comp.GetCache(GroupType) ?? await Comp.LoadCache(GroupType, api).ConfigureAwait(false);
                        var grp = (Uniconta.DataModel.DCGroup)DebGrps.Get(debtor._Group);
                        if (grp != null)
                            _PriceList = grp._PriceList;
                        if (_PriceList == null)
                            UseCustomerPrices = false;
                    }
                }
            }

            if (UseCustomerPrices)
            {
                this.priceLists = Comp.GetCache(PriceListType) ?? await Comp.LoadCache(PriceListType, api).ConfigureAwait(false);
            }

            LoadingTask = null; // we are done
        }
        
         public Task5 DebitorChanged()
        {
            return LoadingTask = LoadBaseData(api);
        }


        public async Task loadPriceList()
        {
            if (UseCustomerPrices && CustomerPrices == null)
            {
                var t = LoadingTask;
                if (t != null && !t.IsCompleted)
                    await t;

                string _plst = _PriceList;
                var n = -priceLists.Count;
                int prioritet = 0;
                bool HasAFirstMatch = false;
                do
                {
                    var plist = (InvPriceList)priceLists.Get(_plst);
                    if (plist == null || _plst == plist._LinkToPricelist)
                        break;

                    if (prioritet == n)
                        break;
                    prioritet--;

                    _plst = plist._LinkToPricelist;
                    if (!plist._Active)
                        continue;

                    var now = plist._UseDeliveryDate ? (order._DeliveryDate != DateTime.MinValue ? order._DeliveryDate : order._Created) : DateTime.MinValue;
                    if (now == DateTime.MinValue)
                        now = DateTime.Now;

                    if ((plist._ValidFrom != DateTime.MinValue && now < plist._ValidFrom) ||
                        (plist._ValidTo != DateTime.MinValue && now > plist._ValidTo))
                        continue;

                    OrderTime = now;

                    var cur = plist._Currency;
                    if (cur == 0)
                        cur = CompCurrency;

                    if (CustomerPrices == null) // first
                        PriceListCurrency = cur;
                    else if (cur != PriceListCurrency)
                        continue;

                    MarginRatio = plist._MarginRatio;
                    SalesCharge = plist._SalesCharge;
                    RoundMargin = plist._RoundMargin;

                    var aggregated = (InvPriceListLine[])(plist.ItemPrices ?? await plist.LoadPrices(this.api));
                    if (aggregated != null)
                    {
                        var FirstMatch = plist._FirstMatch && !HasAFirstMatch;
                        if (FirstMatch)
                            HasAFirstMatch = true;
                        foreach (var p in aggregated)
                        {
                            p._prioritet = prioritet;
                            p._FirstMatch = FirstMatch;
                        }
                        if (CustomerPrices == null)
                            CustomerPrices = (InvPriceListLine[])aggregated;
                        else
                        {
                            var mergeLen = aggregated.Length;
                            var Orglen = CustomerPrices.Length;
                            Array.Resize(ref CustomerPrices, Orglen + mergeLen);
                            Array.Copy(aggregated, 0, CustomerPrices, Orglen, mergeLen);
                        }
                    }
                } while (_plst != null);

                if (CustomerPrices == null)
                {
                    UseCustomerPrices = false;
                    return;
                }

                priceSort = new InvPristListLineSort();
                Array.Sort(CustomerPrices, priceSort);
            }
        }

        Task _SetPriceFromItem(DCOrderLineClient orderLine, InvItem item, double LineMarginRatio, double LineSalesCharge)
        {
            Task t = null;
            byte priceCurrency = item._Currency1;
            var price = item._SalesPrice1;
            var costprice = item._CostPrice;
            var unit = item._SalesUnit;

            var MarginRatio = this.MarginRatio;
            if (LineMarginRatio > 0d)
                MarginRatio = LineMarginRatio;

            if (orderLine.__DCType() == 2)
            {
                unit = item._PurchaseUnit;
                if (item._PurchasePrice != 0d)
                {
                    price = item._PurchasePrice;
                    priceCurrency = item._PurchaseCurrency;
                }
                else
                {
                    price = costprice;
                    priceCurrency = 0;
                }
            }
            else if (MarginRatio > 0d && MarginRatio < 100d)
            {
                priceCurrency = 0;
                var margin = costprice * MarginRatio / (100d - MarginRatio);
                price = Math.Round(costprice + margin, RoundMargin ? 0 : 2) + SalesCharge + LineSalesCharge;
            }
            else
            {
                var pg = this.PriceGrp;
                if (pg == 0)
                {
                    if (OrderCurrency == item._Currency2)
                        pg = 2;
                    else if (OrderCurrency == item._Currency3)
                        pg = 3;
                }
                if (pg == 2)
                {
                    priceCurrency = item._Currency2;
                    price = item._SalesPrice2;
                }
                else if (pg == 3)
                {
                    priceCurrency = item._Currency3;
                    price = item._SalesPrice3;
                }
            }

            if (OrderCurrency != priceCurrency)
            {
                if (priceCurrency == 0)
                    priceCurrency = CompCurrency;

                if (OrderCurrency != priceCurrency)
                {
                    if (priceCurrency == CompCurrency)
                        price = (ExchangeRate == 0d) ? price : Math.Round(price * ExchangeRate, 2);
                    else
                    {
                        t = LookRate(orderLine, price, priceCurrency, OrderCurrency);
                        price = 0;
                    }
                }
            }
            if (orderLine.__DCType() != 2)
                orderLine.SetCostFromItem(item);
            if (price != 0)
                orderLine.Price = price;
            if (unit != 0)
            {
                orderLine._Unit = unit;
                orderLine.NotifyPropertyChanged("Unit");
            }
            return t;
        }

        public Task SetPriceFromItem(DCOrderLineClient orderLine, InvItem item)
        {
            Task t = _SetPriceFromItem(orderLine, item, 0d, 0d);
            Task t2 = null;
            if (this.UseCustomerPrices && this._PriceList != null && orderLine._Item != null)
                t2 = GetCustomerPrice(orderLine, true);
            if (t2 == null)
                return t;
            else if (t == null)
                return t2;
            else if (t.IsCompleted)
                return t2;
            else if (t2.IsCompleted)
                return t;
            else
#if !SILVERLIGHT
                return Task.WhenAll(new Task[] { t, t2 });
#else
                return t2;
#endif
        }
         public Task2 DebitorChanged()
        {
            return LoadingTask = LoadBaseData(api);
        }
        private async Task LookRate(DCOrderLineClient rec, double price, byte From, byte To)
        {
            var Rate = await api.session.ExchangeRate((Currencies)From, (Currencies)To, DateTime.Now, api.CompanyEntity);
            rec.Price = (Rate == 0d) ? price : Math.Round(price * Rate, 2);
        }
        static int VariantCompare(string LineVariant, string PriceVariant)
        {
            if (LineVariant == null && PriceVariant == null)
                return 2;
            if (LineVariant == null && PriceVariant != null)
                return -1;
            if (LineVariant != null && PriceVariant == null)
                return 0;
            return (string.Compare(LineVariant, PriceVariant) == 0) ? 3 : -1;
        }

        int search(InvPriceListLine searchrec)
        {
            var pos = Array.BinarySearch(CustomerPrices, searchrec, priceSort);
            if (pos < 0)
                return ~pos;
            else
                return pos;
        }

        public async Task GetCustomerPrice(DCOrderLineClient orderLine, bool IsNewItem)
        {
            if (!this.UseCustomerPrices)
                return;

            var t = LoadingTask;
            if (t != null && !t.IsCompleted)
                await t;

            if (this._PriceList == null || orderLine._Item == null)
                return;
            var itemRec = (InvItem)items.Get(orderLine._Item);
            if (itemRec == null)
                return;
            orderLine._Qty = Math.Round(orderLine._Qty, itemRec._Decimals);

            if (CustomerPrices == null)
                await loadPriceList();
            if (CustomerPrices == null)
                return;

            var len = CustomerPrices.Length;
            var searchrec = new InvPriceListLine();
            searchrec._DCType = debtor.__DCType();
            searchrec._Item = orderLine._Item;
            // Do not include Variant, since we also want to finde prices without variant
            //searchrec._Variant1 = orderLine._Variant1;
            //searchrec._Variant2 = orderLine._Variant2;
            searchrec._prioritet = int.MinValue;
            var pos = search(searchrec);

            var now = OrderTime;
            int FoundPriority = -1;
            bool HasQtyPrices = false;
            int CurVariantMatch = -1;
            InvPriceListLine rec = null;
            var qty = Math.Abs(orderLine._Qty) + 0.00000000001d;
            while (pos < len)
            {
                var r = CustomerPrices[pos++];
                var c = string.Compare(r._Item, orderLine._Item);
                if (c != 0)
                    break;
                if ((r._ValidFrom != DateTime.MinValue && now < r._ValidFrom) ||
                    (r._ValidTo != DateTime.MinValue && now > r._ValidTo))
                    continue;
                int VariantMatch = VariantCompare(orderLine._Variant1, r._Variant1);
                if (VariantMatch >= 0)
                {
                    int VariantMatch2 = VariantCompare(orderLine._Variant2, r._Variant2);
                    if (VariantMatch2 >= 0)
                    {
                        VariantMatch += VariantMatch2;
                        if (VariantMatch >= CurVariantMatch)
                        {
                            if (r._Qty != 0d)
                                HasQtyPrices = true;
                            if (r._Qty <= qty)
                            {
                                bool UseMatch = true;
                                if (r._FirstMatch)
                                {
                                    if (FoundPriority == -1)
                                        FoundPriority = r._prioritet;
                                    else if (FoundPriority != r._prioritet)
                                        UseMatch = false;
                                }
                                if (UseMatch)
                                {
                                    rec = r;
                                    CurVariantMatch = VariantMatch;
                                }
                            }
                        }
                    }
                }
            }
            CurVariantMatch = 0;
            searchrec._Item = null;
            searchrec._Variant1 = null;
            searchrec._Variant2 = null;

            if (rec == null && itemRec._DiscountGroup != null)
            {
                var discountgroup = itemRec._DiscountGroup;
                searchrec._DiscountGroup = discountgroup;
                pos = search(searchrec);
                while (pos < len)
                {
                    var r = CustomerPrices[pos++];
                    int c = string.Compare(r._DiscountGroup, discountgroup);
                    if (c != 0)
                        break;
                    if ((r._ValidFrom != DateTime.MinValue && now < r._ValidFrom) ||
                        (r._ValidTo != DateTime.MinValue && now > r._ValidTo))
                        continue;
                    if (r._Qty != 0d)
                        HasQtyPrices = true;
                    if (r._Qty <= qty)
                        rec = r;
                }
            }

            if (rec == null && itemRec._Group != null)
            {
                var group = itemRec._Group;
                searchrec._ItemGroup = group;
                searchrec._DiscountGroup = null;
                pos = search(searchrec);
                while (pos < len)
                {
                    var r = CustomerPrices[pos++];
                    int c = string.Compare(r._ItemGroup, group);
                    if (c != 0)
                        break;
                    if ((r._ValidFrom != DateTime.MinValue && now < r._ValidFrom) ||
                        (r._ValidTo != DateTime.MinValue && now > r._ValidTo))
                        continue;
                    if (r._Qty != 0d)
                        HasQtyPrices = true;
                    if (r._Qty <= qty)
                        rec = r;
                }
            }

            if (rec != null && (IsNewItem || HasQtyPrices || CurVariantMatch >= 0))
            {
                if (rec._Pct != 0d)
                    orderLine.DiscountPct = rec._Pct;
                if (rec._Discount != 0d)
                    orderLine.Discount = rec._Discount;
                if (rec._Unit != 0)
                {
                    orderLine._Unit = rec._Unit;
                    orderLine.NotifyPropertyChanged("Unit");
                }

                if (rec._Price != 0d)
                {
                    var price = rec._Price;
                    if (OrderCurrency != PriceListCurrency)
                    {
                        if (PriceListCurrency == CompCurrency)
                            price = (ExchangeRate == 0d) ? price : Math.Round(price * ExchangeRate, 2);
                        else
                        {
                            await LookRate(orderLine, price, PriceListCurrency, OrderCurrency);
                            return;
                        }
                    }
                    orderLine.Price = price;
                }

                if (!rec._FixedContributionRate)
                    return;
            }

            var task = _SetPriceFromItem(orderLine, (InvItem)items.Get(orderLine._Item), rec != null ? rec._ContributionRate : 0d, rec != null ? rec._SalesCharge : 0d);
            if (task != null)
                await task;
        }

        // project
 public Task3 DebitorChanged()
        {
            return LoadingTask = LoadBaseData(api);
        }
        public async Task GetCustomerPrice(ProjectJournalLineClient orderLine, bool IsNewItem)
        {
            if (!this.UseCustomerPrices)
                return;

            var t = LoadingTask;
            if (t != null && !t.IsCompleted)
                await t;

            if (this._PriceList == null || orderLine._Item == null)
                return;
            var itemRec = (InvItem)items.Get(orderLine._Item);
            if (itemRec == null)
                return;
            orderLine._Qty = Math.Round(orderLine._Qty, itemRec._Decimals);

            if (CustomerPrices == null)
                await loadPriceList();
            if (CustomerPrices == null)
                return;

            var len = CustomerPrices.Length;
            var searchrec = new InvPriceListLine();
            searchrec._DCType = debtor.__DCType();
            searchrec._Item = orderLine._Item;
            // Do not include Variant, since we also want to finde prices without variant
            //searchrec._Variant1 = orderLine._Variant1;
            //searchrec._Variant2 = orderLine._Variant2;
            searchrec._prioritet = int.MinValue;
            var pos = search(searchrec);

            var now = OrderTime;
            int FoundPriority = -1;
            bool HasQtyPrices = false;
            int CurVariantMatch = -1;
            InvPriceListLine rec = null;
            var qty = Math.Abs(orderLine._Qty) + 0.00000000001d;
            while (pos < len)
            {
                var r = CustomerPrices[pos++];
                var c = string.Compare(r._Item, orderLine._Item);
                if (c != 0)
                    break;
                if ((r._ValidFrom != DateTime.MinValue && now < r._ValidFrom) ||
                    (r._ValidTo != DateTime.MinValue && now > r._ValidTo))
                    continue;
                int VariantMatch = VariantCompare(orderLine._Variant1, r._Variant1);
                if (VariantMatch >= 0)
                {
                    int VariantMatch2 = VariantCompare(orderLine._Variant2, r._Variant2);
                    if (VariantMatch2 >= 0)
                    {
                        VariantMatch += VariantMatch2;
                        if (VariantMatch >= CurVariantMatch)
                        {
                            if (r._Qty != 0d)
                                HasQtyPrices = true;
                            if (r._Qty <= qty)
                            {
                                bool UseMatch = true;
                                if (r._FirstMatch)
                                {
                                    if (FoundPriority == -1)
                                        FoundPriority = r._prioritet;
                                    else if (FoundPriority != r._prioritet)
                                        UseMatch = false;
                                }
                                if (UseMatch)
                                {
                                    rec = r;
                                    CurVariantMatch = VariantMatch;
                                }
                            }
                        }
                    }
                }
            }

            CurVariantMatch = 0;
            searchrec._Item = null;
            searchrec._Variant1 = null;
            searchrec._Variant2 = null;

            if (rec == null && itemRec._DiscountGroup != null)
            {
                var discountgroup = itemRec._DiscountGroup;
                searchrec._DiscountGroup = discountgroup;
                pos = search(searchrec);
                while (pos < len)
                {
                    var r = CustomerPrices[pos++];
                    int c = string.Compare(r._DiscountGroup, discountgroup);
                    if (c != 0)
                        break;
                    if ((r._ValidFrom != DateTime.MinValue && now < r._ValidFrom) ||
                        (r._ValidTo != DateTime.MinValue && now > r._ValidTo))
                        continue;
                    if (r._Qty != 0d)
                        HasQtyPrices = true;
                    if (r._Qty <= qty)
                        rec = r;
                }
            }

            if (rec == null && itemRec._Group != null)
            {
                var group = itemRec._Group;
                searchrec._ItemGroup = group;
                searchrec._DiscountGroup = null;
                pos = search(searchrec);
                while (pos < len)
                {
                    var r = CustomerPrices[pos++];
                    int c = string.Compare(r._ItemGroup, group);
                    if (c != 0)
                        break;
                    if ((r._ValidFrom != DateTime.MinValue && now < r._ValidFrom) ||
                        (r._ValidTo != DateTime.MinValue && now > r._ValidTo))
                        continue;
                    if (r._Qty != 0d)
                        HasQtyPrices = true;
                    if (r._Qty <= qty)
                        rec = r;
                }
            }

            if (rec != null && (IsNewItem || HasQtyPrices || CurVariantMatch >= 0))
            {
                if (rec._Pct != 0d)
                    orderLine.DiscountPct = rec._Pct;
                if (rec._Discount != 0d && orderLine._SalesPrice >= rec._Discount)
                    orderLine.SalesPrice = orderLine._SalesPrice - rec._Discount;

                if (rec._Price != 0d)
                {
                    var price = rec._Price;
                    if (OrderCurrency != PriceListCurrency)
                    {
                        if (PriceListCurrency == CompCurrency)
                            price = (ExchangeRate == 0d) ? price : Math.Round(price * ExchangeRate, 2);
                        else
                        {
                            await LookRate(orderLine, price, PriceListCurrency, OrderCurrency);
                            return;
                        }
                    }
                    orderLine.SalesPrice = price;
                }

                if (!rec._FixedContributionRate)
                    return;
            }

            var task = _SetPriceFromItem(orderLine, (InvItem)items.Get(orderLine._Item), rec != null ? rec._ContributionRate : 0d, rec != null ? rec._SalesCharge : 0d);
            if (task != null)
                await task;
        }

        Task _SetPriceFromItem(ProjectJournalLineClient orderLine, InvItem item, double LineMarginRatio, double LineSalesCharge)
        {
            Task t = null;
            byte priceCurrency = item._Currency1;
            var price = item._SalesPrice1;
            var costprice = item._CostPrice;

            var MarginRatio = this.MarginRatio;
            if (LineMarginRatio > 0d)
                MarginRatio = LineMarginRatio;

            if (MarginRatio > 0d && MarginRatio < 100d)
            {
                priceCurrency = 0;
                var margin = costprice * MarginRatio / (100d - MarginRatio);
                price = Math.Round(costprice + margin, RoundMargin ? 0 : 2) + SalesCharge + LineSalesCharge;
            }
            else
            {
                var pg = this.PriceGrp;
                if (pg == 0)
                {
                    if (OrderCurrency == item._Currency2)
                        pg = 2;
                    else if (OrderCurrency == item._Currency3)
                        pg = 3;
                }
                if (pg == 2)
                {
                    priceCurrency = item._Currency2;
                    price = item._SalesPrice2;
                }
                else if (pg == 3)
                {
                    priceCurrency = item._Currency3;
                    price = item._SalesPrice3;
                }
            }

            if (OrderCurrency != priceCurrency)
            {
                if (priceCurrency == 0)
                    priceCurrency = CompCurrency;

                if (OrderCurrency != priceCurrency)
                {
                    if (priceCurrency == CompCurrency)
                        price = (ExchangeRate == 0d) ? price : Math.Round(price * ExchangeRate, 2);
                    else
                    {
                        t = LookRate(orderLine, price, priceCurrency, OrderCurrency);
                        price = 0;
                    }
                }
            }
            orderLine.CostPrice = item._CostPrice;
            if (price != 0)
                orderLine.SalesPrice = price;
            return t;
        }

        private async Task LookRate(ProjectJournalLineClient rec, double price, byte From, byte To)
        {
            var Rate = await api.session.ExchangeRate((Currencies)From, (Currencies)To, DateTime.Now, api.CompanyEntity);
            rec.SalesPrice = (Rate == 0d) ? price : Math.Round(price * Rate, 2);
        }

        public Task SetPriceFromItem(ProjectJournalLineClient orderLine, InvItem item)
        {
            Task t = _SetPriceFromItem(orderLine, item, 0d, 0d);
            Task t2 = null;
            if (this.UseCustomerPrices && this._PriceList != null && orderLine._Item != null)
                t2 = GetCustomerPrice(orderLine, true);
            if (t2 == null)
                return t;
            else if (t == null)
                return t2;
            else if (t.IsCompleted)
                return t2;
            else if (t2.IsCompleted)
                return t;
            else
#if !SILVERLIGHT
                return Task.WhenAll(new Task[] { t, t2 });
#else
                return t2;
#endif
        }
    }
}
