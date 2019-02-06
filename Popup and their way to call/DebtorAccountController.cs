using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Uniconta.API.DebtorCreditor;
using Uniconta.API.System;
using Uniconta.ClientTools;
using Uniconta.ClientTools.Controls;
using Uniconta.ClientTools.DataModel;
using Uniconta.Common;
using Uniconta.DataModel;
using Uniconta_Web.CustomHtmlHelpers;
using Uniconta_Web.Models;
using Uniconta_Web.Models.Debitor;
using Uniconta_Web.Models.GL;
using Uniconta_Web.Models.Inventory;
using Uniconta_Web.Reports.PrintReport;
using Uniconta_Web.Utility;
using Uniconta_Web.Utility.Reporting;

namespace Uniconta_Web.Controllers
{
    public class DebtorAccountController : BaseController
    {
        Uniconta.API.DebtorCreditor.FindPrices PriceLookup;
        DateTime filterDate;
        double exchangeRate;

        //SQLCache ItemsCache, WarehouseCache, Variants1Cache, Variants2Cache, StandardVariantsCache;
        #region Debtor Account

        public ActionResult DebtorAccounts()
        {

            TempData.Clear();
            ViewBag.CompanyDetatil = si.CurrentCompany;
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> LoadDebtorAccounts(DataSourceLoadOptions loadOptions, string IsGridRefreshed, string FilterSortValues)
        {
            bool IsRefreshed = false;
            if (!String.IsNullOrEmpty(IsGridRefreshed))
            {
                IsRefreshed = Convert.ToBoolean(IsGridRefreshed);
            }
            List<DebtorPortal> DebtorAccountslist = TempDataManager.LoadTempData(this, "dbAccounts") as List<DebtorPortal>;
            if (IsRefreshed || DebtorAccountslist == null || DebtorAccountslist.Count == 0)
            {
                var res = await FilterGrid(typeof(DebtorPortal), null, null, FilterSortValues, IsRefreshed);
                DebtorAccountslist = GetList(res);
                TempDataManager.SaveTempData(this, "dbAccounts", DebtorAccountslist);
            }
            return Content(JsonConvert.SerializeObject(DataSourceLoader.Load(DebtorAccountslist, loadOptions), new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), "application/json");
        }

        [HttpGet]
        public async Task<ActionResult> Details(int id = 0)
        {
            ViewBag.toolChosen = "Details";
            List<DebtorPortal> DebtorAccountslist = TempDataManager.LoadTempData(this, "dbAccounts") as List<DebtorPortal>;
            var debtor = DebtorAccountslist.Where(d => d.RowId == id).FirstOrDefault();
            if (debtor != null)
            {
                ViewBag.accountNumber = debtor.Account;
                ViewBag.accountName = debtor.Name;
                debtor.CompanyEntity = si.CurrentCompany;
                dropdownListBinding();
                if (si.CurrentCompany.CRM)
                    await GetCrmInterestsAndProduct();
                return View("DebtorAccounts_Model", debtor);
            }
            return RedirectToAction("Login", "Login");
        }

        [HttpGet]
        public async Task<ActionResult> Create()
        {
            ViewBag.toolChosen = "Create";
            var debtorClient = new DebtorPortal();
            debtorClient.CompanyEntity = si.CurrentCompany;
            debtorClient.SetMaster(debtorClient.CompanyEntity);

            dropdownListBinding();
            if (si.CurrentCompany.CRM)
                await GetCrmInterestsAndProduct();
            return PartialView("DebtorAccounts_Model", debtorClient);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(DebtorPortal newdebtordata, FormCollection foc)
        {
            List<DebtorPortal> DebtorAccountslist = TempDataManager.LoadTempData(this, "dbAccounts") as List<DebtorPortal>;
            if (ModelState.IsValid)
            {
                newdebtordata.SetMaster(si.CurrentCompany);
                if (foc["Interests"] != null)
                    newdebtordata.Interests = foc["Interests"].Replace(",", ";");

                if (foc["Products"] != null)
                    newdebtordata.Products = foc["Products"].Replace(",", ";");

                //if (newdebtordata.UserFieldDef() != null)
                //{
                //    string[] data = new string[newdebtordata.UserFieldDef().Length];
                //    for (int i = 0; i < newdebtordata.UserFieldDef().Length; i++)
                //    {
                //        data[i] = foc["UserField._data[" + i + "]"];

                //    }
                //    newdebtordata.UserField._data = data;
                //}

                var err = await DataQuery.Insert(newdebtordata);
                if (err == ErrorCodes.Succes)
                {
                    DebtorAccountslist.Add(newdebtordata);
                    TempDataManager.SaveTempData(this, "dbAccounts", DebtorAccountslist);

                    if (DebtorAccountslist.Count == 1)
                    {
                        return Json(new { success = true, message = string.Format(Localization.lookup("SavedOBJ"), string.Empty), rowid = newdebtordata.RowId, newTable = Localization.lookup("New") });
                    }
                    else
                    {
                        return Json(new { success = true, message = string.Format(Localization.lookup("SavedOBJ"), string.Empty), rowid = newdebtordata.RowId });
                    }
                }
                else
                {
                    return await getJsonError(err);
                }
            }
            else
            {
                return getModelStateError();
            }
        }

        [HttpGet]
        public async Task<ActionResult> Edit(int id = 0)
        {
            ViewBag.toolChosen = "Edit";
            DebtorPortal debtor;
            List<DebtorPortal> DebtorAccountslist = TempDataManager.LoadTempData(this, "dbAccounts") as List<DebtorPortal>;
            if (DebtorAccountslist == null)
            {
                return RedirectToAction("Login", "Login");
            }

            debtor = DebtorAccountslist.Where(d => d.RowId == id).FirstOrDefault();
            if (debtor != null)
            {
                ViewBag.accountNumber = debtor.Account;
                ViewBag.accountName = debtor.Name;
                debtor.CompanyEntity = si.CurrentCompany;
                dropdownListBinding();
                if (si.CurrentCompany.CRM)
                    await GetCrmInterestsAndProduct();
                return View("DebtorAccounts_Model", debtor);
            }
            return RedirectToAction("Login", "Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, DebtorPortal newdebtordata, FormCollection foc)
        {

            if (ModelState.IsValid)
            {
                List<DebtorPortal> DebtorAccountslist = TempDataManager.LoadTempData(this, "dbAccounts") as List<DebtorPortal>;
                DebtorPortal oldDebtor = new DebtorPortal();
                if (DebtorAccountslist != null)
                {
                    oldDebtor = DebtorAccountslist.Where(d => d.RowId == id).FirstOrDefault();
                }
                if (oldDebtor == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                newdebtordata.PricesInclVat = CheckBoolean(newdebtordata.PricesInclVat.ToString().ToLower());
                newdebtordata.SetMaster(si.CurrentCompany);
                if (foc["Interests"] != null)
                    newdebtordata.Interests = foc["Interests"].Replace(",", ";");

                if (foc["Products"] != null)
                    newdebtordata.Products = foc["Products"].Replace(",", ";");

                var err = await DataQuery.Update(oldDebtor, newdebtordata);

                if (err == ErrorCodes.Succes)
                {
                    int indrow = DebtorAccountslist.IndexOf(oldDebtor);
                    DebtorAccountslist.RemoveAt(indrow);
                    DebtorAccountslist.Insert(indrow, newdebtordata);
                    TempDataManager.SaveTempData(this, "dbAccounts", DebtorAccountslist);
                    return Json(new { success = true, message = string.Format(Localization.lookup("SavedOBJ"), string.Empty) });
                }
                else
                {
                    return await getJsonError(err);
                }
            }
            else
                return getModelStateError();
        }

        [HttpGet]
        public async Task<ActionResult> Delete(int id = 0)
        {
            ViewBag.toolChosen = "Delete";
            DebtorPortal debtor;

            List<DebtorPortal> DebtorAccountslist = TempDataManager.LoadTempData(this, "dbAccounts") as List<DebtorPortal>;
            if (DebtorAccountslist == null)
            {
                return RedirectToAction("Login", "Login");
            }
            debtor = DebtorAccountslist.Where(d => d.RowId == id).FirstOrDefault();
            if (debtor != null)
            {
                ViewBag.accountNumber = debtor.Account;
                ViewBag.accountName = debtor.Name;
                debtor.CompanyEntity = si.CurrentCompany;
                dropdownListBinding();
                if (si.CurrentCompany.CRM)
                    await GetCrmInterestsAndProduct();
                return View("DebtorAccounts_Model", debtor);
            }
            return RedirectToAction("Login", "Login");
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            List<DebtorPortal> DebtorAccountslist = TempDataManager.LoadTempData(this, "dbAccounts") as List<DebtorPortal>;
            if (DebtorAccountslist == null)
            {
                return RedirectToAction("Login", "Login");
            }
            var debtor = DebtorAccountslist.Where(d => d.RowId == id).FirstOrDefault();

            if (debtor != null)
            {
                var err = await DataQuery.Delete(debtor);
                if (err == ErrorCodes.Succes)
                {
                    DebtorAccountslist.Remove(debtor);
                    TempDataManager.SaveTempData(this, "dbAccounts", DebtorAccountslist);
                    return Json(new { success = true, message = Localization.lookup("Deleted"), rowid = id });
                }
                else
                {
                    return await getJsonError(err);
                }
            }
            return RedirectToAction("Login", "Login");
        }

        private void dropdownListBinding()
        {
            GetCountries();
            GetVatZones();
            GetCurrencies();
            GetLanguages();
            GetBankAccountTypes();
        }

        #endregion

        #region Transaction
        protected override Uniconta.ClientTools.Controls.Filter[] DefaultFilters()
        {
            if (FilterType == nameof(Controllers.FilterType.DebtorTransClient))
            {
                if (filterDate != DateTime.MinValue)
                {
                    Uniconta.ClientTools.Controls.Filter dateFilter = new Uniconta.ClientTools.Controls.Filter() { name = "Date", value = string.Format("{0:d}..", filterDate) };
                    return new Uniconta.ClientTools.Controls.Filter[] { dateFilter };
                }
            }
            return base.DefaultFilters();
        }
        protected override SortingProperties[] DefaultSort()
        {
            if (FilterType == nameof(Controllers.FilterType.DebtorTransClient))
            {
                SortingProperties dateSort = new SortingProperties("Date") { Ascending = false };
                SortingProperties VoucherSort = new SortingProperties("Voucher");
                return new SortingProperties[] { dateSort, VoucherSort };
            }
            return base.DefaultSort();
        }

        public ActionResult DebtorTransactions(int rowId)
        {
            ViewBag.RowId = rowId;

            //clear ordersLines
            TempDataManager.RemoveTempData(this, "openDepTrans");
            ViewBag.CompanyDetatil = si.CurrentCompany;
            if (rowId != 0)
            {
                List<DebtorPortal> DebtorAccountslist = TempDataManager.LoadTempData(this, "dbAccounts") as List<DebtorPortal>;
                var orderDetailsById = DebtorAccountslist.Where(s => s.RowId == rowId).FirstOrDefault();
                ViewBag.AccountNumber = orderDetailsById.Account;
            }
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> LoadDebtorTransactions(DataSourceLoadOptions loadOptions, string IsGridRefreshed, string FilterSortValues, int rowId = 0)
        {
            bool IsRefreshed = false;
            if (!String.IsNullOrEmpty(IsGridRefreshed))
            {
                IsRefreshed = Convert.ToBoolean(IsGridRefreshed);
            }
            List<DebtorPortal> DebtorAccountslist = TempDataManager.LoadTempData(this, "dbAccounts") as List<DebtorPortal>;
            var acDetailsById = DebtorAccountslist.Where(s => s.RowId == rowId).FirstOrDefault();

            List<DebtorTransClient> DebtorAccountTransacList = TempDataManager.LoadTempData(this, "openDepTrans") as List<DebtorTransClient>;
            if (IsRefreshed || (acDetailsById != null && DebtorAccountTransacList == null) || (acDetailsById != null && DebtorAccountTransacList.Count == 0))
            {
                FilterType = nameof(Controllers.FilterType.DebtorTransClient);
                filterDate = GetFilterDate(si.CurrentCompany, acDetailsById != null);
                //var res = await FilterGrid(typeof(DebtorTransClient), new List<UnicontaBaseEntity>() { acDetailsById });
                var res = await FilterGrid(typeof(DebtorTransClient), new List<UnicontaBaseEntity>() { acDetailsById }, null, FilterSortValues, IsRefreshed);
                DebtorAccountTransacList = res.OfType<DebtorTransClient>().ToList();
                TempDataManager.SaveTempData(this, "openDepTrans", DebtorAccountTransacList);
                return Content(JsonConvert.SerializeObject(DataSourceLoader.Load(DebtorAccountTransacList, loadOptions), new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), "application/json");
            }
            else if (acDetailsById != null && DebtorAccountTransacList != null)
            {
                return Content(JsonConvert.SerializeObject(DataSourceLoader.Load(DebtorAccountTransacList, loadOptions), new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), "application/json");
            }
            return HttpNotFound();
        }

        #endregion

        #region Sales Order

        [HttpGet]
        public ActionResult DebtorOrders(int rowId = 0)
        {
            ViewBag.RowId = rowId;
            ViewBag.IsForDebAccOrder = false;
            ViewBag.CompanyDetatil = si.CurrentCompany;

            TempDataManager.RemoveTempData(this, "dbOrders");
            if (rowId != 0)
            {
                ViewBag.IsForDebAccOrder = true;
                List<DebtorPortal> DebtorAccountslist = TempDataManager.LoadTempData(this, "dbAccounts") as List<DebtorPortal>;
                var orderDetailsById = DebtorAccountslist.Where(s => s.RowId == rowId).FirstOrDefault();
                ViewBag.AccountNumber = orderDetailsById.Account;
            }
            else
            {
                TempData.Clear();
            }
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> LoadDebtorOrders(DataSourceLoadOptions loadOptions, string IsGridRefreshed, string FilterSortValues, int rowId = 0)
        {
            SQLCache debtors = si.CurrentCompany.GetCache(typeof(Debtor));
            if (debtors == null)
                debtors = await si.CurrentCompany.LoadCache(typeof(Debtor), si.BaseAPI).ConfigureAwait(true);

            TempDataManager.RemoveTempData(this, "dbOrders");
            bool IsRefreshed = false;

            //Instance used to manage null list of orders.
            List<DebtorOrderPortal> DebtorOrdersWithEmptylist = new List<DebtorOrderPortal>();

            if (!String.IsNullOrEmpty(IsGridRefreshed))
            {
                IsRefreshed = Convert.ToBoolean(IsGridRefreshed);
            }

            if (rowId == 0)
            {
                List<DebtorOrderPortal> DebtorOrderslist = TempDataManager.LoadTempData(this, "dbOrders") as List<DebtorOrderPortal>;
                if (IsRefreshed || DebtorOrderslist == null || DebtorOrderslist.Count == 0)
                {
                    var res = await FilterGrid(typeof(DebtorOrderPortal), null, null, FilterSortValues, IsRefreshed);
                    DebtorOrderslist = GetList(res);

                    TempDataManager.SaveTempData(this, "dbOrders", DebtorOrderslist);
                    if (DebtorOrderslist != null)

                        return Content(JsonConvert.SerializeObject(DataSourceLoader.Load(DebtorOrderslist, loadOptions), new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), "application/json");
                    else
                        return Content(JsonConvert.SerializeObject(DataSourceLoader.Load(DebtorOrdersWithEmptylist, loadOptions)), "application/json");
                }
                else
                    return Content(JsonConvert.SerializeObject(DataSourceLoader.Load(DebtorOrderslist, loadOptions), new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), "application/json");
            }
            else
            {
                List<DebtorPortal> DebtorAccountslist = TempDataManager.LoadTempData(this, "dbAccounts") as List<DebtorPortal>;
                var orderDetailsById = DebtorAccountslist.Where(s => s.RowId == rowId).FirstOrDefault();
                List<DebtorOrderPortal> DebtorAccountOpenOrdersList = TempDataManager.LoadTempData(this, "dbOrders") as List<DebtorOrderPortal>;
                if (IsRefreshed || (orderDetailsById != null && DebtorAccountOpenOrdersList == null) || (orderDetailsById != null && DebtorAccountOpenOrdersList.Count == 0))
                {
                    var res = await FilterGrid(typeof(DebtorOrderPortal), new List<UnicontaBaseEntity>() { orderDetailsById }, null, FilterSortValues, IsRefreshed);
                    if (res != null)
                    {
                        DebtorAccountOpenOrdersList = res.OfType<DebtorOrderPortal>().ToList();
                        TempDataManager.SaveTempData(this, "dbOrders", DebtorAccountOpenOrdersList);
                        return Content(JsonConvert.SerializeObject(DataSourceLoader.Load(DebtorAccountOpenOrdersList, loadOptions), new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), "application/json");
                    }
                    else
                        return Content(JsonConvert.SerializeObject(DataSourceLoader.Load(DebtorOrdersWithEmptylist, loadOptions)), "application/json");
                }
                else if (orderDetailsById != null && DebtorAccountOpenOrdersList != null)
                {
                    return Content(JsonConvert.SerializeObject(DataSourceLoader.Load(DebtorAccountOpenOrdersList, loadOptions), new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), "application/json");
                }
                return HttpNotFound();
            }
        }

        [HttpGet]
        public ActionResult OrderDetails(int id = 0)
        {
            ViewBag.toolChosen = "OrderDetails";
            List<DebtorOrderPortal> DebtorOrderslist = TempDataManager.LoadTempData(this, "dbOrders") as List<DebtorOrderPortal>;
            var debtorOrder = DebtorOrderslist.Where(d => d.RowId == id).FirstOrDefault();
            if (debtorOrder != null)
            {
                ViewBag.orderNumber = debtorOrder.OrderNumber;
                debtorOrder.CompanyEntity = si.CurrentCompany;

                orderDDListBinding();
                return PartialView("DebtorOrders_Model", debtorOrder);
            }
            return RedirectToAction("Login", "Login");
        }

        [HttpGet]
        public ActionResult CreateOrder()
        {
            ViewBag.toolChosen = "CreateOrder";
            var debtorOrder = new DebtorOrderPortal();
            debtorOrder.CompanyEntity = si.CurrentCompany;
            debtorOrder.SetMaster(debtorOrder.CompanyEntity);

            orderDDListBinding();
            return PartialView("DebtorOrders_Model", debtorOrder);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateOrder(DebtorOrderPortal newDebOrdData, FormCollection frm)
        {
            var userAction = frm.Get("hdnUserAction");

            List<DebtorOrderPortal> DebtorOrderslist = TempDataManager.LoadTempData(this, "dbOrders") as List<DebtorOrderPortal>;

            if (ModelState.IsValid)
            {
                newDebOrdData.SetMaster(si.CurrentCompany);

                var err = await DataQuery.Insert(newDebOrdData);
                if (err == ErrorCodes.Succes)
                {
                    DebtorOrderslist.Add(newDebOrdData);
                    TempDataManager.SaveTempData(this, "dbOrders", DebtorOrderslist);
                    if (userAction.ToLower() == "saveandgotoline")
                    {
                        return Json(new { saveNGoToLine = true, message = string.Format(Localization.lookup("SavedOBJ"), string.Empty), accNo = newDebOrdData.Account, orderNo = newDebOrdData.OrderNumber });
                        //return RedirectToAction("DebtorOrderLines", new { accNo = newDebOrdData.Account, orderNo = newDebOrdData.OrderNumber});
                    }
                    else
                    {
                        if (DebtorOrderslist.Count == 1)
                        {
                            return Json(new { success = true, message = string.Format(Localization.lookup("SavedOBJ"), string.Empty), rowid = newDebOrdData.RowId, newTable = Localization.lookup("New") });
                        }
                        else
                        {
                            return Json(new { success = true, message = string.Format(Localization.lookup("SavedOBJ"), string.Empty), rowid = newDebOrdData.RowId });
                        }
                    }

                }
                else
                {
                    return await getJsonError(err);
                }
            }
            else
            {
                return getModelStateError();
            }
        }

        [HttpGet]
        public ActionResult EditOrder(int id = 0)
        {
            ViewBag.toolChosen = "EditOrder";
            DebtorOrderPortal debtorOrder;
            List<DebtorOrderPortal> DebtorOrderslist = TempDataManager.LoadTempData(this, "dbOrders") as List<DebtorOrderPortal>;
            if (DebtorOrderslist == null)
            {
                return RedirectToAction("Login", "Login");
            }
            debtorOrder = DebtorOrderslist.Where(d => d.RowId == id).FirstOrDefault();
            if (debtorOrder != null)
            {
                ViewBag.orderNumber = debtorOrder.OrderNumber;
                debtorOrder.CompanyEntity = si.CurrentCompany;

                orderDDListBinding();
                return PartialView("DebtorOrders_Model", debtorOrder);
            }
            return RedirectToAction("Login", "Login");
        }

        // POST: /DebtorAccount/Edit/5
        [HttpPost]
        //public async Task<ActionResult> EditOrder(string toolchosen, int id, DebtorOrderPortal newdebtororderdata, FormCollection foc)
        public async Task<ActionResult> EditOrder(int id, DebtorOrderPortal newdebtororderdata, FormCollection frm)
        {
            var userAction = frm.Get("hdnUserAction");


            if (ModelState.IsValid)
            {
                List<DebtorOrderPortal> DebtorOrderslist = TempDataManager.LoadTempData(this, "dbOrders") as List<DebtorOrderPortal>;
                if (DebtorOrderslist == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                DebtorOrderPortal oldDebtorOrder = DebtorOrderslist.Where(d => d.RowId == id).FirstOrDefault();
                if (oldDebtorOrder == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                newdebtororderdata.PricesInclVat = CheckBoolean(newdebtororderdata.PricesInclVat.ToString().ToLower());
                newdebtororderdata.SetMaster(si.CurrentCompany);
                var err = await DataQuery.Update(oldDebtorOrder, newdebtororderdata);
                if (err == ErrorCodes.Succes)
                {
                    int indrow = DebtorOrderslist.IndexOf(oldDebtorOrder);
                    DebtorOrderslist.RemoveAt(indrow);
                    DebtorOrderslist.Insert(indrow, newdebtororderdata);
                    TempDataManager.SaveTempData(this, "dbOrders", DebtorOrderslist);
                    if (userAction.ToLower() == "saveandgotoline")
                    {
                        return Json(new { saveNGoToLine = true, message = string.Format(Localization.lookup("SavedOBJ"), string.Empty), accNo = newdebtororderdata.Account, orderNo = newdebtororderdata.OrderNumber });
                        //return RedirectToAction("DebtorOrderLines", new { accNo = newdebtororderdata.Account, orderNo = newdebtororderdata.OrderNumber });
                    }
                    else
                    {
                        return Json(new { success = true, message = string.Format(Localization.lookup("SavedOBJ"), string.Empty) });
                    }
                }
                else
                {
                    return await getJsonError(err);
                }
            }
            else
                return getModelStateError();
        }

        [HttpGet]
        public ActionResult DeleteOrder(int id = 0)
        {
            ViewBag.toolChosen = "DeleteOrder";
            DebtorOrderPortal debtorOrder;

            List<DebtorOrderPortal> DebtorOrderslist = TempDataManager.LoadTempData(this, "dbOrders") as List<DebtorOrderPortal>;
            if (DebtorOrderslist == null)
            {
                return RedirectToAction("Login", "Login");
            }
            debtorOrder = DebtorOrderslist.Where(d => d.RowId == id).FirstOrDefault();
            if (debtorOrder != null)
            {
                ViewBag.orderNumber = debtorOrder.OrderNumber;
                debtorOrder.CompanyEntity = si.CurrentCompany;
                orderDDListBinding();
                return PartialView("DebtorOrders_Model", debtorOrder);
            }
            return RedirectToAction("Login", "Login");
        }

        [HttpPost, ActionName("DeleteOrder")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteOrderConfirmed(int id)
        {
            List<DebtorOrderPortal> DebtorOrderslist = TempDataManager.LoadTempData(this, "dbOrders") as List<DebtorOrderPortal>;
            if (DebtorOrderslist == null)
            {
                return RedirectToAction("Login", "Login");
            }
            var debtorOrder = DebtorOrderslist.Where(d => d.RowId == id).FirstOrDefault();

            if (debtorOrder != null)
            {
                var err = await DataQuery.Delete(debtorOrder);
                if (err == ErrorCodes.Succes)
                {
                    DebtorOrderslist.Remove(debtorOrder);
                    TempDataManager.SaveTempData(this, "dbOrders", DebtorOrderslist);
                    return Json(new { success = true, message = Localization.lookup("Deleted"), rowid = id });
                }
                else
                {
                    return await getJsonError(err);
                }
            }
            return RedirectToAction("Login", "Login");
        }

        private void orderDDListBinding()
        {
            GetCurrencies();
            GetCountries();
            GetInvoiceIntervals();
        }

        #endregion

        #region Sales Order Lines

        //public async Task<ActionResult> DebtorOrderLines(string accNo, int orderNo = 0)
        public async Task<ActionResult> DebtorOrderLines(string accNo, int orderNo = 0)
        {
            TempDataManager.RemoveTempData(this, "dbOrderLines");
            TempDataManager.RemoveTempData(this, "dbOpenOrder");
            ViewBag.CompanyDetatil = si.CurrentCompany;
            ViewBag.Account = accNo;
            ViewBag.Order = orderNo;
            orderLineDDListBinding();

            if (!string.IsNullOrWhiteSpace(accNo) && orderNo != 0)
            {
                List<DebtorOrderPortal> DebtorOrderslist = TempDataManager.LoadTempData(this, "dbOrders") as List<DebtorOrderPortal>;
                if (DebtorOrderslist == null || DebtorOrderslist.Count == 0)
                {
                    var res = await FilterGrid(typeof(DebtorOrderPortal));
                    DebtorOrderslist = GetList(res);
                    TempDataManager.SaveTempData(this, "dbOrders", DebtorOrderslist);
                }
                var debtorOrder = DebtorOrderslist.Where(d => (d.Account == accNo) && (d.OrderNumber == orderNo)).FirstOrDefault();
                TempDataManager.SaveTempData(this, "dbOpenOrder", debtorOrder);
                PriceLookup = new Uniconta.API.DebtorCreditor.FindPrices(debtorOrder, si.BaseAPI);
                TempDataManager.SaveTempData(this, "priceLookup", PriceLookup);
            }
            //Load cashe for the below fields during the page load.
            //await LoadCacheInBackGroundForDebtor();

            return View();
        }

        //protected async Task LoadCacheInBackGroundForDebtor()
        //{
        //    var api = si.BaseAPI;
        //    var cmp = si.CurrentCompany;

        //    ItemsCache = cmp.GetCache(typeof(InvItem)) ?? await cmp.LoadCache(typeof(InvItem), api).ConfigureAwait(false);
        //    Variants1Cache = si.CurrentCompany.GetCache(typeof(InvVariant1)) ?? await si.CurrentCompany.LoadCache(typeof(InvVariant1), api).ConfigureAwait(false);
        //    Variants2Cache = si.CurrentCompany.GetCache(typeof(InvVariant2)) ?? await si.CurrentCompany.LoadCache(typeof(InvVariant2), api).ConfigureAwait(false);
        //    StandardVariantsCache = si.CurrentCompany.GetCache(typeof(InvStandardVariant)) ?? await si.CurrentCompany.LoadCache(typeof(InvStandardVariant), api).ConfigureAwait(false);
        //    if (cmp.Warehouse)
        //        WarehouseCache = cmp.GetCache(typeof(InvWarehouse)) ?? await cmp.LoadCache(typeof(InvWarehouse), api).ConfigureAwait(false);            
        //}

        [HttpGet]
        public async Task<ActionResult> LoadDebtorOrderLines(DataSourceLoadOptions loadOptions, string IsGridRefreshed, string accNo, int orderNo = 0)
        {
            TempDataManager.RemoveTempData(this, "dbOpenOrderLine");
            Session["curRowId"] = null;
            //Session.Remove("curRowId");

            bool IsRefreshed = false;
            if (!String.IsNullOrEmpty(IsGridRefreshed))
            {
                IsRefreshed = Convert.ToBoolean(IsGridRefreshed);
            }

            List<DebtorOrderPortal> DebtorOrderslist = TempDataManager.LoadTempData(this, "dbOrders") as List<DebtorOrderPortal>;
            var OpenDebtorOrder = DebtorOrderslist.Where(d => (d.Account == accNo) && (d.OrderNumber == orderNo)).FirstOrDefault();

            //For set master during insertion of new line.
            TempDataManager.SaveTempData(this, "dbOpenOrder", OpenDebtorOrder);

            List<DebtorOrderLinePortal> DebtorOrderLinelist = TempDataManager.LoadTempData(this, "dbOrderLines") as List<DebtorOrderLinePortal>;
            if (IsRefreshed || (OpenDebtorOrder != null && DebtorOrderLinelist == null) || (OpenDebtorOrder != null && DebtorOrderLinelist.Count == 0) || (DebtorOrderLinelist.First().OrderNumber != orderNo.ToString()))
            {
                var res = await FilterGrid(typeof(DebtorOrderLinePortal), new List<UnicontaBaseEntity>() { OpenDebtorOrder });
                List<DebtorOrderLinePortal> resultList = res.OfType<DebtorOrderLinePortal>().ToList();
                TempDataManager.SaveTempData(this, "dbOrderLines", resultList);

                return Content(JsonConvert.SerializeObject(DataSourceLoader.Load(resultList, loadOptions), new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), "application/json");
            }
            else if (OpenDebtorOrder != null && DebtorOrderLinelist != null)
            {
                return Content(JsonConvert.SerializeObject(DataSourceLoader.Load(DebtorOrderLinelist, loadOptions), new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), "application/json");
            }
            return HttpNotFound();
        }

        private void orderLineDDListBinding()
        {
            GetUnits();
            GetStorage();
            GetCurrencies();
        }

        [HttpPut]
        public async Task<ActionResult> UpdateOrderLine(FormCollection form)
        {
            var values = form.Get("values");
            var key = Convert.ToInt32(form.Get("key"));



            // Converting the JSON into a Sub Delivery Order object that belongs to the model
            List<DebtorOrderLinePortal> DebtorOrderLinelist = TempDataManager.LoadTempData(this, "dbOrderLines") as List<DebtorOrderLinePortal>;
            var oldDeptorOrderline = DebtorOrderLinelist.Where(d => d.RowId == key).FirstOrDefault();
            if (oldDeptorOrderline == null)
            {
                return RedirectToAction("Login", "Login");
            }
            if (ModelState.IsValid)
            {
                var clone = StreamingManager.Clone(oldDeptorOrderline) as DebtorOrderLinePortal;
                JsonConvert.PopulateObject(values, clone);
                var err = await DataQuery.Update(oldDeptorOrderline, clone);
                if (err == ErrorCodes.Succes)
                {
                    int indrow = DebtorOrderLinelist.IndexOf(oldDeptorOrderline);
                    DebtorOrderLinelist.RemoveAt(indrow);
                    DebtorOrderLinelist.Insert(indrow, clone);
                    TempDataManager.SaveTempData(this, "dbOrderLines", DebtorOrderLinelist);
                    return Json(new { success = true, message = string.Format(Localization.lookup("SavedOBJ"), string.Empty) });
                }
                else
                    orderLineDDListBinding();
                return await getJsonError(err);
            }
            else
            {
                return getModelStateError();
            }
        }

        [HttpPost]
        public async Task<ActionResult> InsertOrderLine(FormCollection form)
        {
            SQLCache debtorOrders = si.CurrentCompany.GetCache(typeof(DebtorOrder));
            if (debtorOrders == null)
                debtorOrders = await si.CurrentCompany.LoadCache(typeof(DebtorOrder), si.BaseAPI).ConfigureAwait(true);

            List<DebtorOrderLinePortal> DebtorOrderLinelist = TempDataManager.LoadTempData(this, "dbOrderLines") as List<DebtorOrderLinePortal>;

            DebtorOrderLinePortal debtorOrderLine = new DebtorOrderLinePortal();



            //To Fetch the current open order
            DebtorOrderPortal OpenDeptorOrder = TempDataManager.LoadTempData(this, "dbOpenOrder") as DebtorOrderPortal;

            if (ModelState.IsValid)
            {
                var values = form.Get("values");
                JsonConvert.PopulateObject(values, debtorOrderLine);
                debtorOrderLine.SetMaster(OpenDeptorOrder);
                var err = await DataQuery.Insert(debtorOrderLine);
                if (err == ErrorCodes.Succes)
                {
                    DebtorOrderLinelist.Add(debtorOrderLine);
                    TempDataManager.SaveTempData(this, "dbOrderLines", DebtorOrderLinelist);
                    if (DebtorOrderLinelist.Count == 1)
                    {
                        return Json(new { Insertsuccess = true, message = string.Format(Localization.lookup("SavedOBJ"), string.Empty), rowid = debtorOrderLine.RowId, newTable = Localization.lookup("New") });
                    }
                    else
                    {
                        return Json(new { Insertsuccess = true, message = string.Format(Localization.lookup("SavedOBJ"), string.Empty), rowid = debtorOrderLine.RowId });
                    }
                }
                else
                {
                    orderLineDDListBinding();
                    return await getJsonError(err);
                }
            }
            else
            {
                return getModelStateError();
            }
        }

        //[HttpPost]
        public async Task<ActionResult> DeleteOrderLine(FormCollection form)
        {
            var key = Convert.ToInt32(form.Get("key"));


            List<DebtorOrderLinePortal> DebtorOrderLinelist = TempDataManager.LoadTempData(this, "dbOrderLines") as List<DebtorOrderLinePortal>;
            var deptorOrderlineToDelete = DebtorOrderLinelist.Where(d => d.RowId == key).FirstOrDefault();
            if (deptorOrderlineToDelete != null)
            {
                var err = await DataQuery.Delete(deptorOrderlineToDelete);
                if (err == ErrorCodes.Succes)
                {
                    DebtorOrderLinelist.Remove(deptorOrderlineToDelete);
                    TempDataManager.SaveTempData(this, "dbOrderLines", DebtorOrderLinelist);
                    return Json(new { DeleteSuccess = true, message = Localization.lookup("Deleted"), rowid = key });
                }
                else
                {
                    return await getJsonError(err);
                }
            }

            return null;
        }

        public async Task<ActionResult> DebtorOrderLineGrid_PropertyChanged(string propertyName, string value, DataSourceLoadOptions loadOptions)
        {
            if (propertyName == "Warehouse")
                return await setLocation(value, loadOptions);
            if (propertyName == "Item")
                return await setVariant(value, false, null, loadOptions);
            else
                return null;
        }

        public async Task<ActionResult> setDependedField(string value, string propertyName, int id = 0)
        {
            var api = si.BaseAPI;
            DebtorOrderLinePortal res = new DebtorOrderLinePortal();
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Error = (serializer, err) => err.ErrorContext.Handled = true;

            try
            {
                string str = string.Empty;
                if (Session["curRowId"] != null)
                {
                    str = Session["curRowId"].ToString();
                }

                DebtorOrderLinePortal OpenDBOrderLine = TempDataManager.LoadTempData(this, "dbOpenOrderLine") as DebtorOrderLinePortal;

                if (OpenDBOrderLine != null && str != id.ToString())
                {
                    TempDataManager.RemoveTempData(this, "dbOpenOrderLine");
                    OpenDBOrderLine = TempDataManager.LoadTempData(this, "dbOpenOrderLine") as DebtorOrderLinePortal;
                }

                if (id != 0 && OpenDBOrderLine == null && propertyName != "Item")
                {
                    Session["curRowId"] = id;
                    List<DebtorOrderLinePortal> DebtorOrderLinelist = TempDataManager.LoadTempData(this, "dbOrderLines") as List<DebtorOrderLinePortal>;
                    var cloneVal = DebtorOrderLinelist.Where(d => d.RowId == id).FirstOrDefault();
                    res = StreamingManager.Clone(cloneVal) as DebtorOrderLinePortal;
                }
                else if ((id == 0 && OpenDBOrderLine == null) || propertyName == "Item")
                {
                    Session["curRowId"] = id;
                    DebtorOrderPortal OpenDeptorOrder = TempDataManager.LoadTempData(this, "dbOpenOrder") as DebtorOrderPortal;
                    var serializedVal = JsonConvert.SerializeObject(OpenDeptorOrder, settings);
                    //Note:-Account and company id missed here, have to manage it by setting account type.
                    JsonConvert.PopulateObject(serializedVal, res, settings);
                }
                else
                {
                    res = OpenDBOrderLine;
                }

                if (propertyName == "Item")
                {
                    res._Variant1 = null;
                    res._Variant2 = null;
                    res._SerieBatch = null;
                    res._Qty = 0;
                    await setItem(value, res);
                }
                else if (propertyName == "Qty")
                {
                    await ManagePriceLookUpTask();

                    res._Qty = Convert.ToDouble(value);
                    if (this.PriceLookup != null && this.PriceLookup.UseCustomerPrices)
                        await this.PriceLookup.GetCustomerPrice(res, false);

                    if (si.BaseAPI.CompanyEntity._InvoiceUseQtyNow)
                        res._QtyNow = res._Qty;
                }
                else if (propertyName == "Subtotal" || propertyName == "Total")
                {
                    res._AmountEntered = res.Total = Convert.ToDouble(value);
                    //await RecalculateAmount(res);
                }
                else if (propertyName == "EAN")
                {
                    SQLCache items = si.CurrentCompany.GetCache(typeof(InvItem)) ?? await si.CurrentCompany.LoadCache(typeof(InvItem), api).ConfigureAwait(false);
                    res._EAN = value;
                    await FindOnEAN(res, items, api);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            //return Json(new { result = JsonConvert.SerializeObject(res, JsonSettings) }, JsonRequestBehavior.AllowGet);
            //Below code is used when want to send whole model
            TempDataManager.SaveTempData(this, "dbOpenOrderLine", res);
            var jsonResult = Json(new { result = JsonConvert.SerializeObject(res, JsonSettings) }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        async Task setItem(string value, DebtorOrderLinePortal res)
        {
            try
            {
                SQLCache items;
                var api = si.BaseAPI;
                items = si.CurrentCompany.GetCache(typeof(InvItem)) ?? await si.CurrentCompany.LoadCache(typeof(InvItem), api).ConfigureAwait(false);
                var selectedItem = (InvItem)items.Get(value);

                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.Error = (serializer, err) => err.ErrorContext.Handled = true;
                var values = JsonConvert.SerializeObject(selectedItem, settings);
                JsonConvert.PopulateObject(values, res, settings);

                if (items != null)
                {
                    if (selectedItem != null)
                    {
                        if (selectedItem._AlternativeItem != null && selectedItem._UseAlternative == UseAlternativeItem.Always)
                        {
                            var altItem = (InvItem)items.Get(selectedItem._AlternativeItem);
                            if (altItem != null && altItem._AlternativeItem == null)
                            {
                                res._Item = selectedItem._AlternativeItem;
                                return;
                            }
                        }
                        if (selectedItem._SalesQty != 0d)
                            res._Qty = selectedItem._SalesQty;
                        else if (api.CompanyEntity._OrderLineOne)
                            res._Qty = 1d;
                        res.SetItemValues(selectedItem, api.CompanyEntity._OrderLineStorage);

                        await ManagePriceLookUpTask();
                        Task t = this.PriceLookup.SetPriceFromItem(res, selectedItem);
                        if (t != null)
                            await t;

                        if (si.CurrentCompany._InvoiceUseQtyNow)
                            res._QtyNow = res._Qty;

                        if (selectedItem._StandardVariant != res.standardVariant)
                        {
                            res._Variant1 = null;
                            res._Variant2 = null;
                            res.variant2Source = null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task ManagePriceLookUpTask()
        {
            Uniconta.API.DebtorCreditor.FindPrices fp = TempDataManager.LoadTempData(this, "priceLookup") as Uniconta.API.DebtorCreditor.FindPrices;
            PriceLookup = fp;

            if (this.PriceLookup != null)
            {
                var t = this.PriceLookup.ExchangeTask;
                this.exchangeRate = this.PriceLookup.ExchangeRate;
                if (this.exchangeRate == 0d && t != null)
                    this.exchangeRate = await t.ConfigureAwait(false);
            }
        }

        //public static DateTime SystemDate;
        //static public DateTime GetSystemDefaultDate()
        //{
        //    if (SystemDate != DateTime.MinValue)
        //        return SystemDate;
        //    else
        //        return DateTime.Now.Date;
        //}

        public async Task<ActionResult> setLocation(string value, DataSourceLoadOptions loadOptions)
        {
            var api = si.BaseAPI;
            SQLCache warehouse = si.CurrentCompany.GetCache(typeof(InvWarehouse));
            if (warehouse == null)
                warehouse = await si.CurrentCompany.LoadCache(typeof(InvWarehouse), api).ConfigureAwait(false);

            DebtorOrderLinePortal rec = new DebtorOrderLinePortal();
            var master = (InvWarehouseClient)warehouse.Get(value, typeof(InvWarehouseClient));

            //Load all location when warehouse i.e valu is null
            if (string.IsNullOrWhiteSpace(value))
            {
                InvWarehouseClient allLocaitonList = new InvWarehouseClient();
                rec.locationSource = await allLocaitonList.LoadLocations(api);
            }
            else if (!string.IsNullOrEmpty(value) && master != null)
            {
                if (master.Locations != null)
                    rec.locationSource = master.Locations;
                else
                    rec.locationSource = await master.LoadLocations(api);
            }
            else
            {
                rec.locationSource = null;
                rec.Location = null;
            }

            var IdKeyNames = rec.locationSource as IList<IdKeyName>;
            List<IdKeyClient> pairs;
            if (IdKeyNames != null)
                pairs = (from s in IdKeyNames select new IdKeyClient { KeyName = s.KeyName, KeyStr = s.KeyStr }).ToList();
            else
            {
                var IdKeys = ((IList<IdKey>)rec.locationSource);
                pairs = (from s in IdKeys select new IdKeyClient { KeyStr = s.KeyStr, KeyName = string.Empty }).ToList();
            }
            return Content(JsonConvert.SerializeObject(DataSourceLoader.Load(pairs, loadOptions)), "application/json");
        }

        public async Task<ActionResult> setVariant(string value, bool SetVariant2, string vr1, DataSourceLoadOptions loadOptions)
        {
            SQLCache items, variants1, variants2, standardVariants;
            var api = si.BaseAPI;
            items = si.CurrentCompany.GetCache(typeof(InvItem));
            variants1 = si.CurrentCompany.GetCache(typeof(InvVariant1));
            variants2 = si.CurrentCompany.GetCache(typeof(InvVariant2));
            standardVariants = si.CurrentCompany.GetCache(typeof(InvStandardVariant));

            DebtorOrderLinePortal rec = new DebtorOrderLinePortal();
            if (items == null)
                items = await si.CurrentCompany.LoadCache(typeof(InvItem), api).ConfigureAwait(false);
            if (variants1 == null)
                variants1 = await si.CurrentCompany.LoadCache(typeof(InvVariant1), api).ConfigureAwait(false);

            if (variants2 == null)
                variants2 = await si.CurrentCompany.LoadCache(typeof(InvVariant2), api).ConfigureAwait(false);

            if (standardVariants == null)
                standardVariants = await si.CurrentCompany.LoadCache(typeof(InvStandardVariant), api).ConfigureAwait(false);

            List<IdKeyClient> var1List = new List<IdKeyClient>();
            List<IdKeyClient> var2List = new List<IdKeyClient>();
            List<IdKey> variant1Pairs = new List<IdKey>();
            List<IdKey> variant2Pairs = new List<IdKey>();
            string LastVariant = null;
            var item = (InvItem)items.Get(value);

            InvStandardVariantClient master = new InvStandardVariantClient();
            IEnumerable<InvStandardVariantCombiClient> Combinations;

            if ((item != null && item._StandardVariant != null) || item == null)
            {
                if (item != null && item._StandardVariant != null)
                {
                    rec.standardVariant = item._StandardVariant;
                    master = (InvStandardVariantClient)standardVariants.Get(item._StandardVariant, typeof(InvStandardVariantClient));

                    if (master == null)
                        return null;
                }

                Combinations = master.Combinations;
                if (Combinations == null)
                {
                    Combinations = await master.LoadCombinations(api);
                    if (Combinations == null)
                    {
                        if (SetVariant2)
                        {
                            var IdKeys = ((IList<IdKey>)variant2Pairs);
                            var2List = (from s in IdKeys select new IdKeyClient { KeyStr = s.KeyStr, KeyName = string.Empty }).ToList();
                        }
                        else
                        {
                            var IdKeys = ((IList<IdKey>)variant1Pairs);
                            var1List = (from s in IdKeys select new IdKeyClient { KeyStr = s.KeyStr, KeyName = string.Empty }).ToList();
                        }
                    }
                }
                if (Combinations != null)
                {
                    foreach (var comb in Combinations)
                    {
                        if (SetVariant2)
                        {
                            InvStandardVariantCombiClient var1 = new InvStandardVariantCombiClient();
                            if (!string.IsNullOrWhiteSpace(vr1))
                            {
                                var1 = Combinations.Where(d => d.Variant1 == vr1).FirstOrDefault();
                                if (comb._Variant2 != null && var1._Variant1 == comb._Variant1)
                                {
                                    var v2 = variants2.Get(comb._Variant2);
                                    variant2Pairs.Add(v2);
                                }
                            }
                            else
                            {
                                if (comb._Variant2 != null)
                                {
                                    var v2 = variants2.Get(comb._Variant2);
                                    variant2Pairs.Add(v2);
                                }
                            }
                        }
                        else if (variants1 != null && LastVariant != comb._Variant1)
                        {
                            LastVariant = comb._Variant1;
                            var v1 = variants1.Get(comb._Variant1);
                            variant1Pairs.Add(v1);
                        }
                    }
                    if (SetVariant2)
                        var2List = (from s in variant2Pairs select new IdKeyClient { KeyStr = s.KeyStr, KeyName = s.KeyName }).ToList();
                    else
                        var1List = (from s in variant1Pairs select new IdKeyClient { KeyStr = s.KeyStr, KeyName = s.KeyName }).ToList();
                }
                else
                {
                    rec.variant1Source = null;
                    rec.variant2Source = null;
                }
            }
            if (SetVariant2)
                return Content(JsonConvert.SerializeObject(DataSourceLoader.Load(var2List, loadOptions)), "application/json");
            else
                return Content(JsonConvert.SerializeObject(DataSourceLoader.Load(var1List, loadOptions)), "application/json");
        }

        public async Task<ActionResult> setSerieBatch(DataSourceLoadOptions loadOptions, string value, int rowId = 0)
        {
            int id = Convert.ToInt32(rowId);
            SQLCache items;
            var serieBatchDisplayText = new List<IdKeyClient>();
            var api = si.BaseAPI;
            items = si.CurrentCompany.GetCache(typeof(InvItem));

            //When value i.e item is null and rowId is also null.
            if (string.IsNullOrWhiteSpace(value) && id == 0)
            {
                var res1 = await api.Query<SerialToOrderLineClient>();
                serieBatchDisplayText = (from s in res1 select new IdKeyClient { KeyStr = (s as SerialToOrderLineClient)?.DisplayText }).ToList();
                return Content(JsonConvert.SerializeObject(DataSourceLoader.Load(serieBatchDisplayText, loadOptions)), "application/json");
            }

            DebtorOrderLinePortal row = new DebtorOrderLinePortal();
            List<DebtorOrderLinePortal> DebtorOrderLinelist = TempDataManager.LoadTempData(this, "dbOrderLines") as List<DebtorOrderLinePortal>;
            row = DebtorOrderLinelist.Where(d => d.RowId == id).FirstOrDefault();
            var item = (InvItem)items.Get(value);

            if (item == null)
                return null;
            if (row != null && row.SerieBatches != null && row.SerieBatches.First()._Item == value)/*Bind if Item changed*/
            {
                serieBatchDisplayText = (from s in row.SerieBatches select new IdKeyClient { KeyStr = (s as SerialToOrderLineClient)?.DisplayText }).ToList();
                return Content(JsonConvert.SerializeObject(DataSourceLoader.Load(serieBatchDisplayText, loadOptions)), "application/json");
            }
            List<UnicontaBaseEntity> masters = null;
            if (row != null && row._Qty < 0)
            {
                masters = new List<UnicontaBaseEntity>() { item };
            }
            else
            {
                // We only select opens
                var mast = new InvSerieBatchOpen();
                mast.SetMaster(item);
                masters = new List<UnicontaBaseEntity>() { mast };
            }
            var res = await api.Query<SerialToOrderLineClient>(masters, null);
            if (res != null && res.Length > 0)
            {
                if (row != null)
                {
                    row.SerieBatches = res;
                    //row.NotifyPropertyChanged("SerieBatches");
                    serieBatchDisplayText = (from s in row.SerieBatches select new IdKeyClient { KeyStr = (s as SerialToOrderLineClient)?.DisplayText }).ToList();
                }
                else
                {
                    DebtorOrderLinePortal row1 = new DebtorOrderLinePortal();
                    row1.SerieBatches = res;
                    //row1.NotifyPropertyChanged("SerieBatches");
                    serieBatchDisplayText = (from s in row1.SerieBatches select new IdKeyClient { KeyStr = (s as SerialToOrderLineClient)?.DisplayText }).ToList();
                }
                return Content(JsonConvert.SerializeObject(DataSourceLoader.Load(serieBatchDisplayText, loadOptions)), "application/json");
            }
            return Content(JsonConvert.SerializeObject(DataSourceLoader.Load(serieBatchDisplayText, loadOptions)), "application/json");
        }

        static public async Task FindOnEAN(DCOrderLineClient rec, SQLCache Items, QueryAPI api)
        {
            var EAN = rec._EAN;
            if (string.IsNullOrWhiteSpace(EAN))
                return;
            var found = (from item in (InvItem[])Items.GetNotNullArray where string.Compare(item._EAN, EAN, StringComparison.CurrentCultureIgnoreCase) == 0 select item).FirstOrDefault();
            if (found != null)
            {
                rec._EAN = found._EAN;
                rec._Item = found._Item;
            }
            else
                await FindOnEANVariant(rec, api);
        }

        static async Task FindOnEANVariant(DCOrderLineClient rec, QueryAPI api)
        {
            var ap = new Uniconta.API.Inventory.ReportAPI(api);
            var variant = await ap.GetInvVariantDetail(rec._EAN);
            if (variant != null)
            {
                rec._Item = variant._Item;
                rec._Variant1 = variant._Variant1;
                rec._Variant2 = variant._Variant2;
                rec._EAN = variant._EAN;
                if (variant._CostPrice != 0d)
                    rec._CostPrice = variant._CostPrice;
            }
        }

        //public async Task<ActionResult> RecalculateAmount(int id)
        //public async Task<ActionResult> RecalculateAmount(DebtorOrderLinePortal res)
        //{
        //    await ManagePriceLookUpTask();
        //    //List<DebtorOrderLinePortal> DebtorOrderLinelist = TempDataManager.LoadTempData(this, "dbOrderLines") as List<DebtorOrderLinePortal>;
        //    //var oldDeptorOrderline = DebtorOrderLinelist.Where(d => d.RowId == id).ToList();
        //    var ret = RecalculateLineSum(res, exRate);
        //    double Amountsum = ret.Item1;
        //    double Costsum = ret.Item2;
        //    double AmountsumCompCur = ret.Item3;
        //    return Json(Amountsum, JsonRequestBehavior.AllowGet);
        //}

        //static public Tuple<double, double, double> RecalculateLineSum(IList source, double exRate = 0d)
        //{
        //    double lastTotal = 0d;
        //    double Amountsum = 0d;
        //    double Costsum = 0d;
        //    if (source != null)
        //    {
        //        foreach (var lin in source)
        //        {
        //            var orderLine = (DCOrderLineClient)lin;
        //            if (!orderLine._Subtotal)
        //            {
        //                var cost = orderLine.costvalue();
        //                Costsum += cost;
        //                var sales = orderLine._Amount;
        //                Amountsum += sales;
        //                if (exRate != 0)
        //                    orderLine._ExchangeRate = exRate;
        //                else
        //                    exRate = orderLine._ExchangeRate;
        //            }
        //            else
        //            {
        //                var subtotal = Amountsum - lastTotal;
        //                if (subtotal != orderLine._AmountEntered)
        //                {
        //                    orderLine._AmountEntered = subtotal;
        //                    if (orderLine._Price != 0d)
        //                        orderLine.Price = 0d; // this will redraw screen 
        //                    //else
        //                    //    orderLine.NotifyPropertyChanged(nameof(orderLine.Total));
        //                }
        //                lastTotal = Amountsum;
        //            }
        //        }
        //    }
        //    double AmountsumCur = exRate == 0d ? Amountsum : Math.Round(Amountsum / exRate, 2);
        //    Costsum = Math.Round(Costsum, 2);
        //    return Tuple.Create(Amountsum, Costsum, AmountsumCur);
        //}
        #endregion

        #region Debtor Account Group

        public ActionResult DebtorGroup()
        {

            TempData.Clear();
            ViewBag.CompanyDetatil = si.CurrentCompany;
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> LoadDebtorGroups(DataSourceLoadOptions loadOptions, string IsGridRefreshed)
        {
            bool IsRefreshed = false;
            if (!String.IsNullOrEmpty(IsGridRefreshed))
            {
                IsRefreshed = Convert.ToBoolean(IsGridRefreshed);
            }
            List<DebtorGroupClient> DebtorGrouplist = TempDataManager.LoadTempData(this, "dbGroups") as List<DebtorGroupClient>;
            if (IsRefreshed || DebtorGrouplist == null || DebtorGrouplist.Count == 0)
            {
                var res = await FilterGrid(typeof(DebtorGroupClient));
                DebtorGrouplist = GetList(res);
                TempDataManager.SaveTempData(this, "dbGroups", DebtorGrouplist);
            }
            return Content(JsonConvert.SerializeObject(DataSourceLoader.Load(DebtorGrouplist, loadOptions), new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), "application/json");
        }

        [HttpGet]
        public ActionResult DepGroupDetails(int id = 0)
        {
            ViewBag.toolChosen = "GroupDetails";
            List<DebtorGroupClient> DebtorGrouplist = TempDataManager.LoadTempData(this, "dbGroups") as List<DebtorGroupClient>;
            var debGroup = DebtorGrouplist.Where(d => d.RowId == id).FirstOrDefault();
            if (debGroup != null)
            {
                ViewBag.groupName = debGroup.Name;

                ViewBag.VatOpt = si.CurrentCompany._UseVatOperation;
                GetTurnOver();
                return View("DebtorGroup_Model", debGroup);
            }
            return RedirectToAction("Login", "Login");
        }

        [HttpGet]
        public ActionResult CreateDepGroup()
        {
            ViewBag.toolChosen = "CreateGroup";
            var debtorGrpClient = new DebtorGroupClient();

            ViewBag.VatOpt = si.CurrentCompany._UseVatOperation;
            GetTurnOver();
            return PartialView("DebtorGroup_Model", debtorGrpClient);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateDepGroup(DebtorGroupClient newdebtorgrpdata, FormCollection foc)
        {
            List<DebtorGroupClient> DebtorGrouplist = TempDataManager.LoadTempData(this, "dbGroups") as List<DebtorGroupClient>;
            if (ModelState.IsValid)
            {


                var err = await DataQuery.Insert(newdebtorgrpdata);
                if (err == ErrorCodes.Succes)
                {
                    DebtorGrouplist.Add(newdebtorgrpdata);
                    TempDataManager.SaveTempData(this, "dbGroups", DebtorGrouplist);

                    if (DebtorGrouplist.Count == 1)
                    {
                        return Json(new { success = true, message = string.Format(Localization.lookup("SavedOBJ"), string.Empty), rowid = newdebtorgrpdata.RowId, newTable = Localization.lookup("New") });
                    }
                    else
                    {
                        return Json(new { success = true, message = string.Format(Localization.lookup("SavedOBJ"), string.Empty), rowid = newdebtorgrpdata.RowId });
                    }
                }
                else
                {
                    return await getJsonError(err);
                }
            }
            else
            {
                return getModelStateError();
            }
        }

        [HttpGet]
        public ActionResult EditDepGroup(int id = 0)
        {
            ViewBag.toolChosen = "EditGroup";
            DebtorGroupClient debtorGroup;

            List<DebtorGroupClient> DebtorGroupList = TempDataManager.LoadTempData(this, "dbGroups") as List<DebtorGroupClient>;
            if (DebtorGroupList == null)
            {
                return RedirectToAction("Login", "Login");
            }
            debtorGroup = DebtorGroupList.Where(d => d.RowId == id).FirstOrDefault();

            if (debtorGroup != null)
            {
                ViewBag.groupName = debtorGroup.Name;

                GetTurnOver();
                ViewBag.VatOpt = si.CurrentCompany._UseVatOperation;
                return View("DebtorGroup_Model", debtorGroup);
            }
            return RedirectToAction("Login", "Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditDepGroup(int id, DebtorGroupClient newDebtorGroupData)
        {

            if (ModelState.IsValid)
            {
                List<DebtorGroupClient> DebtorGroupList = TempDataManager.LoadTempData(this, "dbGroups") as List<DebtorGroupClient>;
                DebtorGroupClient oldDebtorGroup = new DebtorGroupClient();
                if (DebtorGroupList != null)
                {
                    oldDebtorGroup = DebtorGroupList.Where(d => d.RowId == id).FirstOrDefault();
                }
                if (oldDebtorGroup == null)
                {
                    return RedirectToAction("Login", "Login");
                }

                var err = await DataQuery.Update(oldDebtorGroup, newDebtorGroupData);

                if (err == ErrorCodes.Succes)
                {
                    int indrow = DebtorGroupList.IndexOf(oldDebtorGroup);
                    DebtorGroupList.RemoveAt(indrow);
                    DebtorGroupList.Insert(indrow, newDebtorGroupData);
                    TempDataManager.SaveTempData(this, "dbGroups", DebtorGroupList);
                    return Json(new { success = true, message = string.Format(Localization.lookup("SavedOBJ"), string.Empty) });
                }
                else
                {
                    return await getJsonError(err);
                }
            }
            else
                return getModelStateError();
        }

        [HttpGet]
        public ActionResult DeleteDepGroup(int id = 0)
        {
            ViewBag.toolChosen = "DeleteGroup";
            DebtorGroupClient debtorGroup;

            List<DebtorGroupClient> DebtorGroupList = TempDataManager.LoadTempData(this, "dbGroups") as List<DebtorGroupClient>;
            if (DebtorGroupList == null)
            {
                return RedirectToAction("Login", "Login");
            }
            debtorGroup = DebtorGroupList.Where(d => d.RowId == id).FirstOrDefault();
            if (debtorGroup != null)
            {
                ViewBag.groupName = debtorGroup.Name;

                ViewBag.VatOpt = si.CurrentCompany._UseVatOperation;
                return View("DebtorGroup_Model", debtorGroup);
            }
            return RedirectToAction("Login", "Login");
        }

        [HttpPost, ActionName("DeleteDepGroup")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteDepGroupConfirmed(int id)
        {
            List<DebtorGroupClient> DebtorGroupList = TempDataManager.LoadTempData(this, "dbGroups") as List<DebtorGroupClient>;
            if (DebtorGroupList == null)
            {
                return RedirectToAction("Login", "Login");
            }
            var debtor = DebtorGroupList.Where(d => d.RowId == id).FirstOrDefault();

            if (debtor != null)
            {
                var err = await DataQuery.Delete(debtor);
                if (err == ErrorCodes.Succes)
                {
                    DebtorGroupList.Remove(debtor);
                    TempDataManager.SaveTempData(this, "dbGroups", DebtorGroupList);
                    return Json(new { success = true, message = Localization.lookup("Deleted"), rowid = id });
                }
                else
                {
                    return await getJsonError(err);
                }
            }
            return RedirectToAction("Login", "Login");
        }



        #endregion

        #region Debtor Order Group

        public ActionResult DebtorOrderGroup()
        {

            TempData.Clear();
            ViewBag.CompanyDetatil = si.CurrentCompany;
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> LoadDebtorOrdGroups(DataSourceLoadOptions loadOptions, string IsGridRefreshed)
        {
            bool IsRefreshed = false;
            if (!String.IsNullOrEmpty(IsGridRefreshed))
            {
                IsRefreshed = Convert.ToBoolean(IsGridRefreshed);
            }
            List<DebtorOrderGroupClient> DebtorOrderGrouplist = TempDataManager.LoadTempData(this, "dbOrdGroups") as List<DebtorOrderGroupClient>;
            if (IsRefreshed || DebtorOrderGrouplist == null || DebtorOrderGrouplist.Count == 0)
            {
                var res = await FilterGrid(typeof(DebtorOrderGroupClient));
                DebtorOrderGrouplist = GetList(res);
                TempDataManager.SaveTempData(this, "dbOrdGroups", DebtorOrderGrouplist);
            }
            return Content(JsonConvert.SerializeObject(DataSourceLoader.Load(DebtorOrderGrouplist, loadOptions), new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), "application/json");
        }

        [HttpPut]
        public async Task<ActionResult> UpdateDebtorOrdGroup(FormCollection form)
        {
            var values = form.Get("values");
            var key = Convert.ToInt32(form.Get("key"));



            List<DebtorOrderGroupClient> DebtorOrderGroupList = TempDataManager.LoadTempData(this, "dbOrdGroups") as List<DebtorOrderGroupClient>;
            var oldDeptorOrderGroupLine = DebtorOrderGroupList.Where(d => d.RowId == key).FirstOrDefault();
            if (oldDeptorOrderGroupLine == null)
            {
                return RedirectToAction("Login", "Login");
            }
            if (ModelState.IsValid)
            {
                var clone = StreamingManager.Clone(oldDeptorOrderGroupLine) as DebtorOrderGroupClient;
                JsonConvert.PopulateObject(values, clone);
                var err = await DataQuery.Update(oldDeptorOrderGroupLine, clone);
                if (err == ErrorCodes.Succes)
                {
                    int indrow = DebtorOrderGroupList.IndexOf(oldDeptorOrderGroupLine);
                    DebtorOrderGroupList.RemoveAt(indrow);
                    DebtorOrderGroupList.Insert(indrow, clone);
                    TempDataManager.SaveTempData(this, "dbOrdGroups", DebtorOrderGroupList);
                    return Json(new { success = true, message = string.Format(Localization.lookup("SavedOBJ"), string.Empty) });
                    //return Content(string.Format(Localization.lookup("SavedOBJ")));
                }
                else
                    return await getJsonError(err);
            }
            else
            {
                return getModelStateError();
            }
        }

        [HttpPost]
        public async Task<ActionResult> InsertDebtorOrdGroup(FormCollection form)
        {
            var values = form.Get("values");
            var debtorOrderLine = new DebtorOrderGroupClient();



            List<DebtorOrderGroupClient> DebtorOrderGroupList = TempDataManager.LoadTempData(this, "dbOrdGroups") as List<DebtorOrderGroupClient>;

            if (ModelState.IsValid)
            {
                JsonConvert.PopulateObject(values, debtorOrderLine);
                var err = await DataQuery.Insert(debtorOrderLine);
                if (err == ErrorCodes.Succes)
                {
                    DebtorOrderGroupList.Add(debtorOrderLine);
                    TempDataManager.SaveTempData(this, "dbOrdGroups", DebtorOrderGroupList);
                    if (DebtorOrderGroupList.Count == 1)
                    {
                        return Json(new { Insertsuccess = true, message = string.Format(Localization.lookup("SavedOBJ"), string.Empty), rowid = debtorOrderLine.RowId, newTable = Localization.lookup("New") });
                    }
                    else
                    {
                        return Json(new { Insertsuccess = true, message = string.Format(Localization.lookup("SavedOBJ"), string.Empty), rowid = debtorOrderLine.RowId });
                    }
                }
                else
                {
                    return await getJsonError(err);
                }
            }
            else
            {
                return getModelStateError();
            }
        }


        public async Task<ActionResult> DeleteDebtorOrdGroup(FormCollection form)
        {
            var key = Convert.ToInt32(form.Get("key"));


            List<DebtorOrderGroupClient> DebtorOrderGroupList = TempDataManager.LoadTempData(this, "dbOrdGroups") as List<DebtorOrderGroupClient>;
            var deptorOrderGroupToDelete = DebtorOrderGroupList.Where(d => d.RowId == key).FirstOrDefault();
            if (deptorOrderGroupToDelete != null)
            {
                var err = await DataQuery.Delete(deptorOrderGroupToDelete);
                if (err == ErrorCodes.Succes)
                {
                    DebtorOrderGroupList.Remove(deptorOrderGroupToDelete);
                    TempDataManager.SaveTempData(this, "dbOrdGroups", DebtorOrderGroupList);
                    return Json(new { DeleteSuccess = true, message = Localization.lookup("Deleted"), rowid = key });
                }
                else
                {
                    return await getJsonError(err);
                }
            }

            return null;
        }



        #endregion

        #region Debtor User Notes
        [HttpGet]
        public ActionResult DebUserNotes(int rowId = 0)
        {
            ViewBag.RowId = rowId;


            TempDataManager.RemoveTempData(this, "debUserNotes");
            TempDataManager.RemoveTempData(this, "debAccountMaster");
            if (rowId != 0)
            {
                List<DebtorPortal> DebtorAccountslist = TempDataManager.LoadTempData(this, "dbAccounts") as List<DebtorPortal>;
                var debAccountById = DebtorAccountslist.Where(s => s.RowId == rowId).FirstOrDefault();
                ViewBag.AccountNumber = debAccountById.Account;
                ViewBag.AccountName = debAccountById.Name;
            }
            return View("DebtorNotesInfo");
        }

        [HttpGet]
        public async Task<ActionResult> LoadDebUserNotes(DataSourceLoadOptions loadOptions, string IsGridRefreshed, int rowId = 0)
        {
            bool IsRefreshed = false;
            if (!String.IsNullOrEmpty(IsGridRefreshed))
            {
                IsRefreshed = Convert.ToBoolean(IsGridRefreshed);
            }

            List<DebtorPortal> DebtorAccountslist = TempDataManager.LoadTempData(this, "dbAccounts") as List<DebtorPortal>;
            DebtorPortal debAccountById = DebtorAccountslist.Where(d => d.RowId == rowId).FirstOrDefault();
            TempDataManager.SaveTempData(this, "debAccountMaster", debAccountById);
            List<UserNotesClient> debUserNotesList = TempDataManager.LoadTempData(this, "debUserNotes") as List<UserNotesClient>;

            if (IsRefreshed || (debAccountById != null && debUserNotesList == null) || (debAccountById != null && debUserNotesList.Count == 0))
            {
                //List<UnicontaBaseEntity> masters = new List<UnicontaBaseEntity>();
                //masters.Add(debAccountById);
                //var res = await FilterGrid(typeof(UserNotesClient), masters);

                var res = await FilterGrid(typeof(UserNotesClient), new List<UnicontaBaseEntity>() { debAccountById });
                if (res != null)
                {
                    //debUserNotesList = res.OfType<UserNotesClient>().ToList();
                    debUserNotesList = GetList(res, typeof(UserNotesClient));
                    TempDataManager.SaveTempData(this, "debUserNotes", debUserNotesList);
                    return Content(JsonConvert.SerializeObject(DataSourceLoader.Load(debUserNotesList, loadOptions), new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), "application/json");
                }
            }
            else if (debAccountById != null && debUserNotesList != null)
            {
                return Content(JsonConvert.SerializeObject(DataSourceLoader.Load(debUserNotesList, loadOptions), new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), "application/json");
            }
            return HttpNotFound();
        }

        [HttpGet]
        public ActionResult CreateDebUserNotes()
        {
            ViewBag.toolChosen = "Create";
            var debUserNote = new UserNotesClient();



            debUserNote.Created = DateTime.Now;
            return View("DebtorNotesInfo_Model", debUserNote);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateDebUserNotes(UserNotesClient newDebtorNoteData)
        {
            List<UserNotesClient> debUserNotesList = TempDataManager.LoadTempData(this, "debUserNotes") as List<UserNotesClient>;
            DebtorPortal debAccount = TempDataManager.LoadTempData(this, "debAccountMaster") as DebtorPortal;
            if (ModelState.IsValid)
            {

                newDebtorNoteData.SetMaster(debAccount);
                var err = await DataQuery.Insert(newDebtorNoteData);
                if (err == ErrorCodes.Succes)
                {
                    debUserNotesList.Add(newDebtorNoteData);
                    TempDataManager.SaveTempData(this, "debUserNotes", debUserNotesList);
                    //return Json(new { success = true, message = string.Format(Localization.lookup("SavedOBJ"), string.Empty), rowid = newDebtorNoteData.TableRowId });
                    return Json(new { success = true, message = string.Format(Localization.lookup("SavedOBJ"), string.Empty) });
                }
                else
                    return await getJsonError(err);
            }
            else
            {
                return getModelStateError();
            }

        }

        public ActionResult EditDebUserNotes(DateTime? cd = null)
        {
            ViewBag.toolChosen = "Edit";
            ViewBag.dateCreated = cd;
            List<UserNotesClient> debUserNotesList = TempDataManager.LoadTempData(this, "debUserNotes") as List<UserNotesClient>;
            var objUserNote = debUserNotesList.Where(d => d.Created == cd).FirstOrDefault();
            if (objUserNote != null)
            {
                return View("DebtorNotesInfo_Model", objUserNote);
            }
            return RedirectToAction("Login", "Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditDebUserNotes(UserNotesClient objUserNotes, FormCollection form)
        {
            DateTime cd = DateTime.Parse(form["hdnCD1"]);


            if (ModelState.IsValid)
            {
                List<UserNotesClient> lstDebUserNotes = TempDataManager.LoadTempData(this, "debUserNotes") as List<UserNotesClient>;
                var oldDebUserNote = lstDebUserNotes.Where(d => d.Created == cd).FirstOrDefault();
                if (oldDebUserNote == null || lstDebUserNotes == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                var clone = StreamingManager.Clone(oldDebUserNote) as UserNotesClient;
                var values = JsonConvert.SerializeObject(form.AllKeys.ToDictionary(k => k, k => form[k]));
                JsonConvert.PopulateObject(values, clone);
                clone.Created = oldDebUserNote.Created;
                clone._Created = oldDebUserNote._Created;
                var err = await DataQuery.Update(oldDebUserNote, clone);

                if (err == ErrorCodes.Succes)
                {
                    int indrow = lstDebUserNotes.IndexOf(oldDebUserNote);
                    lstDebUserNotes.RemoveAt(indrow);
                    lstDebUserNotes.Insert(indrow, clone);
                    TempDataManager.SaveTempData(this, "debUserNotes", lstDebUserNotes);
                    return Json(new { success = true, message = string.Format(Localization.lookup("SavedOBJ"), string.Empty) });
                }
                else
                {
                    return await getJsonError(err);
                }
            }
            else
                return getModelStateError();
        }

        public ActionResult DeleteDebUserNotes(DateTime? cd = null)
        {

            ViewBag.toolChosen = "Delete";
            ViewBag.dateCreated = cd;
            List<UserNotesClient> lstDebUserNotes = TempDataManager.LoadTempData(this, "debUserNotes") as List<UserNotesClient>;
            var debUserNote = lstDebUserNotes.Where(d => d.Created == cd).FirstOrDefault();
            if (debUserNote != null)
            {
                return View("DebtorNotesInfo_Model", debUserNote);
            }
            return RedirectToAction("Login", "Login");
        }

        [HttpPost, ActionName("DeleteDebUserNotes")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteUserNotes(FormCollection form)
        {
            DateTime cd = DateTime.Parse(form["hdnCD1"]);
            List<UserNotesClient> lstDebUserNotes = TempDataManager.LoadTempData(this, "debUserNotes") as List<UserNotesClient>;
            var debUserNote = lstDebUserNotes.Where(d => d.Created == cd).FirstOrDefault();

            if (debUserNote != null)
            {
                var err = await DataQuery.Delete(debUserNote);
                if (err == ErrorCodes.Succes)
                {
                    lstDebUserNotes.Remove(debUserNote);
                    TempDataManager.SaveTempData(this, "debUserNotes", lstDebUserNotes);
                    return Json(new { success = true, message = Localization.lookup("Deleted"), string.Empty });
                }
                else
                {
                    return await getJsonError(err);
                }
            }
            else
            {
                return RedirectToAction("Login", "Login");
            }
        }


        #endregion

        #region Debtor Document
        [HttpGet]
        public ActionResult DebDocumentsInfo(int rowId = 0)
        {
            ViewBag.RowId = rowId;

            //clear ordersLines
            TempDataManager.RemoveTempData(this, "debDocumentsInfo");
            TempDataManager.RemoveTempData(this, "debAccountMaster");
            if (rowId != 0)
            {
                List<DebtorPortal> DebtorAccountslist = TempDataManager.LoadTempData(this, "dbAccounts") as List<DebtorPortal>;
                var debAccountById = DebtorAccountslist.Where(s => s.RowId == rowId).FirstOrDefault();
                ViewBag.AccountNumber = debAccountById.Account;
                ViewBag.AccountName = debAccountById.Name;
            }
            return View("DebDocumentsInfo");
        }

        [HttpGet]
        public async Task<ActionResult> LoadDebDocumentsInfo(DataSourceLoadOptions loadOptions, string IsGridRefreshed, int rowId = 0)
        {
            bool IsRefreshed = false;
            if (!String.IsNullOrEmpty(IsGridRefreshed))
            {
                IsRefreshed = Convert.ToBoolean(IsGridRefreshed);
            }

            List<DebtorPortal> DebtorAccountsList = TempDataManager.LoadTempData(this, "dbAccounts") as List<DebtorPortal>;
            DebtorPortal debAccountById = DebtorAccountsList.Where(d => d.RowId == rowId).FirstOrDefault();
            TempDataManager.SaveTempData(this, "debAccountMaster", debAccountById);
            List<UserDocsClient> debDocumentsList = TempDataManager.LoadTempData(this, "debDocumentsInfo") as List<UserDocsClient>;

            if (IsRefreshed || (debAccountById != null && debDocumentsList == null) || (debAccountById != null && debDocumentsList.Count == 0))
            {
                var res = await FilterGrid(typeof(UserDocsClient), new List<UnicontaBaseEntity>() { debAccountById });
                if (res != null)
                {
                    debDocumentsList = GetList(res, typeof(UserDocsClient));
                    TempDataManager.SaveTempData(this, "debDocumentsInfo", debDocumentsList);
                    return Content(JsonConvert.SerializeObject(DataSourceLoader.Load(debDocumentsList, loadOptions), new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), "application/json");
                }
            }
            else if (debAccountById != null && debDocumentsList != null)
            {
                return Content(JsonConvert.SerializeObject(DataSourceLoader.Load(debDocumentsList, loadOptions), new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), "application/json");
            }
            return RedirectToAction("Login", "Login");
        }

        [HttpGet]
        public ActionResult CreateDebDocumentsInfo()
        {
            ViewBag.toolChosen = "Create";


            var debUserDocument = new UserDocsClient();
            debUserDocument.Created = DateTime.Now;
            return View("DebDocumentsInfo_Model", debUserDocument);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateDebDocumentsInfo(HttpPostedFileBase files, FormCollection form)
        {

            List<UserDocsClient> debDocumentsInfoList = TempDataManager.LoadTempData(this, "debDocumentsInfo") as List<UserDocsClient>;
            DebtorPortal debAccount = TempDataManager.LoadTempData(this, "debAccountMaster") as DebtorPortal;
            UserDocsClient objUserDocument = new UserDocsClient();
            objUserDocument.SetMaster(debAccount);
            objUserDocument.Text = form["Text"];
            objUserDocument.Group = form["Group"];
            objUserDocument.DocumentType = DocumentConvert.GetDocumentType(Path.GetExtension(files.FileName));
            if (ModelState.IsValid)
            {
                MemoryStream target = new MemoryStream();
                files.InputStream.CopyTo(target);
                byte[] fileBytes = target.ToArray();
                if (fileBytes.Length <= TableAddOnData.MaxDocSize && fileBytes.Length > 0)
                {
                    objUserDocument.UserDocument = fileBytes;
                }
                var err = await DataQuery.Insert(objUserDocument);
                if (err == ErrorCodes.Succes)
                {
                    debDocumentsInfoList.Add(objUserDocument);
                    TempDataManager.SaveTempData(this, "debDocumentsInfo", debDocumentsInfoList);
                    return Json(new { success = true, message = string.Format(Localization.lookup("SavedOBJ"), string.Empty) });
                }
                else
                    return await getJsonError(err);
            }
            else
            {
                return getModelStateError();
            }
        }

        public ActionResult EditDebDocumentsInfo(DateTime? cd = null)
        {
            ViewBag.toolChosen = "Edit";
            ViewBag.dateCreated = cd;
            List<UserDocsClient> debDocumentsInfoList = TempDataManager.LoadTempData(this, "debDocumentsInfo") as List<UserDocsClient>;
            var objUserDocument = debDocumentsInfoList.Where(d => d.Created == cd).FirstOrDefault();
            if (objUserDocument != null)
            {

                return View("DebDocumentsInfo_Model", objUserDocument);
            }
            return RedirectToAction("Login", "Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditDebDocumentsInfo(UserDocsClient objDocumentsInfo, FormCollection form)
        {
            DateTime cd = DateTime.Parse(form["hdnCD1"]);


            if (ModelState.IsValid)
            {
                List<UserDocsClient> debDocumentsInfoList = TempDataManager.LoadTempData(this, "debDocumentsInfo") as List<UserDocsClient>;
                var oldUserDocument = debDocumentsInfoList.Where(d => d.Created == cd).FirstOrDefault();
                if (oldUserDocument == null || debDocumentsInfoList == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                var clone = StreamingManager.Clone(oldUserDocument) as UserDocsClient;
                var values = JsonConvert.SerializeObject(form.AllKeys.ToDictionary(k => k, k => form[k]));
                JsonConvert.PopulateObject(values, clone);
                clone.Created = oldUserDocument.Created;
                clone._Created = oldUserDocument._Created;
                var err = await DataQuery.Update(oldUserDocument, clone);
                if (err == ErrorCodes.Succes)
                {
                    int indrow = debDocumentsInfoList.IndexOf(oldUserDocument);
                    debDocumentsInfoList.RemoveAt(indrow);
                    debDocumentsInfoList.Insert(indrow, clone);
                    TempDataManager.SaveTempData(this, "debDocumentsInfo", debDocumentsInfoList);

                    return Json(new { success = true, message = string.Format(Localization.lookup("SavedOBJ"), string.Empty) });
                }
                else
                    return await getJsonError(err);
            }
            else
            {
                return getModelStateError();
            }
        }

        public ActionResult DeleteDebDocumentsInfo(DateTime? cd = null)
        {

            ViewBag.toolChosen = "Delete";
            ViewBag.dateCreated = cd;
            List<UserDocsClient> debDocumentsInfoList = TempDataManager.LoadTempData(this, "debDocumentsInfo") as List<UserDocsClient>;
            var dbUserDocument = debDocumentsInfoList.Where(d => d.Created == cd).FirstOrDefault();
            if (dbUserDocument != null)
            {
                return View("DebDocumentsInfo_Model", dbUserDocument);
            }
            return RedirectToAction("Login", "Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteDebDocumentsInfo(FormCollection form)
        {
            DateTime cd = DateTime.Parse(form["hdnCD1"]);

            List<UserDocsClient> debDocumentsInfoList = TempDataManager.LoadTempData(this, "debDocumentsInfo") as List<UserDocsClient>;
            var dbUserDocument = debDocumentsInfoList.Where(d => d.Created == cd).FirstOrDefault();

            if (dbUserDocument != null)
            {
                var err = await DataQuery.Delete(dbUserDocument);
                if (err == ErrorCodes.Succes)
                {
                    debDocumentsInfoList.Remove(dbUserDocument);
                    TempDataManager.SaveTempData(this, "debDocumentsInfo", debDocumentsInfoList);
                    return Json(new { success = true, message = Localization.lookup("Deleted"), string.Empty });
                }
                else
                {
                    return await getJsonError(err);
                }
            }
            else
            {
                return RedirectToAction("Login", "Login");
            }
        }


        #endregion

        #region Debtor Order User Notes
        [HttpGet]
        public ActionResult DebOrdUserNotes(int rowId = 0)
        {
            ViewBag.RowId = rowId;


            TempDataManager.RemoveTempData(this, "debOrdUserNotes");
            TempDataManager.RemoveTempData(this, "debOrdAccountMaster");
            if (rowId != 0)
            {
                List<DebtorOrderPortal> DebtorOrderslist = TempDataManager.LoadTempData(this, "dbOrders") as List<DebtorOrderPortal>;
                var debAccountById = DebtorOrderslist.Where(s => s.RowId == rowId).FirstOrDefault();
                ViewBag.AccountNumber = debAccountById.Account;
                ViewBag.AccountName = debAccountById.Name;
            }
            return View("DebOrderNotesInfo");
        }

        [HttpGet]
        public async Task<ActionResult> LoadDebOrdUserNotes(DataSourceLoadOptions loadOptions, string IsGridRefreshed, int rowId = 0)
        {
            bool IsRefreshed = false;
            if (!String.IsNullOrEmpty(IsGridRefreshed))
            {
                IsRefreshed = Convert.ToBoolean(IsGridRefreshed);
            }

            List<DebtorOrderPortal> DebtorOrdersList = TempDataManager.LoadTempData(this, "dbOrders") as List<DebtorOrderPortal>;
            DebtorOrderPortal debAccountById = DebtorOrdersList.Where(d => d.RowId == rowId).FirstOrDefault();
            TempDataManager.SaveTempData(this, "debOrdAccountMaster", debAccountById);
            List<UserNotesClient> debUserNotesList = TempDataManager.LoadTempData(this, "debOrdUserNotes") as List<UserNotesClient>;

            if (IsRefreshed || (debAccountById != null && debUserNotesList == null) || (debAccountById != null && debUserNotesList.Count == 0))
            {
                var res = await FilterGrid(typeof(UserNotesClient), new List<UnicontaBaseEntity>() { debAccountById });
                if (res != null)
                {
                    debUserNotesList = GetList(res, typeof(UserNotesClient));
                    TempDataManager.SaveTempData(this, "debOrdUserNotes", debUserNotesList);
                    return Content(JsonConvert.SerializeObject(DataSourceLoader.Load(debUserNotesList, loadOptions), new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), "application/json");
                }
            }
            else if (debAccountById != null && debUserNotesList != null)
            {
                return Content(JsonConvert.SerializeObject(DataSourceLoader.Load(debUserNotesList, loadOptions), new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), "application/json");
            }
            return RedirectToAction("Login", "Login");
        }

        [HttpGet]
        public ActionResult CreateDebOrdUserNotes()
        {
            ViewBag.toolChosen = "Create";
            var debUserNote = new UserNotesClient();



            debUserNote.Created = DateTime.Now;
            return View("DebOrderNotesInfo_Model", debUserNote);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateDebOrdUserNotes(UserNotesClient newDebtorNoteData)
        {
            List<UserNotesClient> debUserNotesList = TempDataManager.LoadTempData(this, "debOrdUserNotes") as List<UserNotesClient>;
            DebtorOrderPortal debAccount = TempDataManager.LoadTempData(this, "debOrdAccountMaster") as DebtorOrderPortal;
            if (ModelState.IsValid)
            {

                newDebtorNoteData.SetMaster(debAccount);
                var err = await DataQuery.Insert(newDebtorNoteData);
                if (err == ErrorCodes.Succes)
                {
                    debUserNotesList.Add(newDebtorNoteData);
                    TempDataManager.SaveTempData(this, "debOrdUserNotes", debUserNotesList);
                    return Json(new { success = true, message = string.Format(Localization.lookup("SavedOBJ"), string.Empty) });
                }
                else
                    return await getJsonError(err);
            }
            else
            {
                return getModelStateError();
            }

        }

        public ActionResult EditDebOrdUserNotes(DateTime? cd = null)
        {
            ViewBag.toolChosen = "Edit";
            ViewBag.dateCreated = cd;
            List<UserNotesClient> debUserNotesList = TempDataManager.LoadTempData(this, "debOrdUserNotes") as List<UserNotesClient>;
            var objUserNote = debUserNotesList.Where(d => d.Created == cd).FirstOrDefault();
            if (objUserNote != null)
            {
                return View("DebOrderNotesInfo_Model", objUserNote);
            }
            return RedirectToAction("Login", "Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditDebOrdUserNotes(UserNotesClient objUserNotes, FormCollection form)
        {
            DateTime cd = DateTime.Parse(form["hdnCD1"]);


            if (ModelState.IsValid)
            {
                List<UserNotesClient> lstDebUserNotes = TempDataManager.LoadTempData(this, "debOrdUserNotes") as List<UserNotesClient>;
                var oldDebUserNote = lstDebUserNotes.Where(d => d.Created == cd).FirstOrDefault();
                if (oldDebUserNote == null || lstDebUserNotes == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                var clone = StreamingManager.Clone(oldDebUserNote) as UserNotesClient;
                var values = JsonConvert.SerializeObject(form.AllKeys.ToDictionary(k => k, k => form[k]));
                JsonConvert.PopulateObject(values, clone);
                clone.Created = oldDebUserNote.Created;
                clone._Created = oldDebUserNote._Created;
                var err = await DataQuery.Update(oldDebUserNote, clone);

                if (err == ErrorCodes.Succes)
                {
                    int indrow = lstDebUserNotes.IndexOf(oldDebUserNote);
                    lstDebUserNotes.RemoveAt(indrow);
                    lstDebUserNotes.Insert(indrow, clone);
                    TempDataManager.SaveTempData(this, "debOrdUserNotes", lstDebUserNotes);
                    return Json(new { success = true, message = string.Format(Localization.lookup("SavedOBJ"), string.Empty) });
                }
                else
                {
                    return await getJsonError(err);
                }
            }
            else
                return getModelStateError();
        }

        public ActionResult DeleteDebOrdUserNotes(DateTime? cd = null)
        {

            ViewBag.toolChosen = "Delete";
            ViewBag.dateCreated = cd;
            List<UserNotesClient> lstDebUserNotes = TempDataManager.LoadTempData(this, "debOrdUserNotes") as List<UserNotesClient>;
            var debUserNote = lstDebUserNotes.Where(d => d.Created == cd).FirstOrDefault();
            if (debUserNote != null)
            {
                return View("DebOrderNotesInfo_Model", debUserNote);
            }
            return RedirectToAction("Login", "Login");
        }

        [HttpPost, ActionName("DeleteDebOrdUserNotes")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteDOUserNotes(FormCollection form)
        {
            DateTime cd = DateTime.Parse(form["hdnCD1"]);
            List<UserNotesClient> lstDebUserNotes = TempDataManager.LoadTempData(this, "debOrdUserNotes") as List<UserNotesClient>;
            var debUserNote = lstDebUserNotes.Where(d => d.Created == cd).FirstOrDefault();

            if (debUserNote != null)
            {
                var err = await DataQuery.Delete(debUserNote);
                if (err == ErrorCodes.Succes)
                {
                    lstDebUserNotes.Remove(debUserNote);
                    TempDataManager.SaveTempData(this, "debOrdUserNotes", lstDebUserNotes);
                    return Json(new { success = true, message = Localization.lookup("Deleted"), string.Empty });
                }
                else
                {
                    return await getJsonError(err);
                }
            }
            else
            {
                return RedirectToAction("Login", "Login");
            }
        }


        #endregion

        #region Debtor Order Document
        [HttpGet]
        public ActionResult DebOrdDocumentsInfo(int rowId = 0)
        {
            ViewBag.RowId = rowId;

            //clear ordersLines
            TempDataManager.RemoveTempData(this, "debOrdDocumentsInfo");
            TempDataManager.RemoveTempData(this, "debOrdAccountMaster");
            if (rowId != 0)
            {
                List<DebtorOrderPortal> DebtorOrdersList = TempDataManager.LoadTempData(this, "dbOrders") as List<DebtorOrderPortal>;
                var debAccountById = DebtorOrdersList.Where(s => s.RowId == rowId).FirstOrDefault();
                ViewBag.AccountNumber = debAccountById.Account;
                ViewBag.AccountName = debAccountById.Name;
            }
            return View("DebOrderDocumentsInfo");
        }

        [HttpGet]
        public async Task<ActionResult> LoadDebOrdDocumentsInfo(DataSourceLoadOptions loadOptions, string IsGridRefreshed, int rowId = 0)
        {
            bool IsRefreshed = false;
            if (!String.IsNullOrEmpty(IsGridRefreshed))
            {
                IsRefreshed = Convert.ToBoolean(IsGridRefreshed);
            }

            List<DebtorOrderPortal> DebtorOrdersList = TempDataManager.LoadTempData(this, "dbOrders") as List<DebtorOrderPortal>;
            DebtorOrderPortal debAccountById = DebtorOrdersList.Where(d => d.RowId == rowId).FirstOrDefault();
            TempDataManager.SaveTempData(this, "debOrdAccountMaster", debAccountById);
            List<UserDocsClient> debDocumentsList = TempDataManager.LoadTempData(this, "debOrdDocumentsInfo") as List<UserDocsClient>;

            if (IsRefreshed || (debAccountById != null && debDocumentsList == null) || (debAccountById != null && debDocumentsList.Count == 0))
            {
                var res = await FilterGrid(typeof(UserDocsClient), new List<UnicontaBaseEntity>() { debAccountById });
                if (res != null)
                {
                    debDocumentsList = GetList(res, typeof(UserDocsClient));
                    TempDataManager.SaveTempData(this, "debOrdDocumentsInfo", debDocumentsList);
                    return Content(JsonConvert.SerializeObject(DataSourceLoader.Load(debDocumentsList, loadOptions), new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), "application/json");
                }
            }
            else if (debAccountById != null && debDocumentsList != null)
            {
                return Content(JsonConvert.SerializeObject(DataSourceLoader.Load(debDocumentsList, loadOptions), new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), "application/json");
            }
            return RedirectToAction("Login", "Login");
        }

        [HttpGet]
        public ActionResult CreateDebOrdDocumentsInfo()
        {
            ViewBag.toolChosen = "Create";


            var debUserDocument = new UserDocsClient();
            debUserDocument.Created = DateTime.Now;
            return View("DebOrderDocuments_Model", debUserDocument);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateDebOrdDocumentsInfo(HttpPostedFileBase files, FormCollection form)
        {

            List<UserDocsClient> debDocumentsInfoList = TempDataManager.LoadTempData(this, "debOrdDocumentsInfo") as List<UserDocsClient>;
            DebtorOrderPortal debAccount = TempDataManager.LoadTempData(this, "debOrdAccountMaster") as DebtorOrderPortal;
            UserDocsClient objUserDocument = new UserDocsClient();
            objUserDocument.SetMaster(debAccount);
            objUserDocument.Text = form["Text"];
            objUserDocument.Group = form["Group"];
            objUserDocument.DocumentType = DocumentConvert.GetDocumentType(Path.GetExtension(files.FileName));
            if (ModelState.IsValid)
            {
                MemoryStream target = new MemoryStream();
                files.InputStream.CopyTo(target);
                byte[] fileBytes = target.ToArray();
                if (fileBytes.Length <= TableAddOnData.MaxDocSize && fileBytes.Length > 0)
                {
                    objUserDocument.UserDocument = fileBytes;
                }
                var err = await DataQuery.Insert(objUserDocument);
                if (err == ErrorCodes.Succes)
                {
                    debDocumentsInfoList.Add(objUserDocument);
                    TempDataManager.SaveTempData(this, "debOrdDocumentsInfo", debDocumentsInfoList);
                    return Json(new { success = true, message = string.Format(Localization.lookup("SavedOBJ"), string.Empty) });
                }
                else
                    return await getJsonError(err);
            }
            else
            {
                return getModelStateError();
            }
        }

        public ActionResult EditDebOrdDocumentsInfo(DateTime? cd = null)
        {
            ViewBag.toolChosen = "Edit";
            ViewBag.dateCreated = cd;
            List<UserDocsClient> debDocumentsInfoList = TempDataManager.LoadTempData(this, "debOrdDocumentsInfo") as List<UserDocsClient>;
            var objUserDocument = debDocumentsInfoList.Where(d => d.Created == cd).FirstOrDefault();
            if (objUserDocument != null)
            {
                return View("DebOrderDocuments_Model", objUserDocument);
            }
            return RedirectToAction("Login", "Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditDebOrdDocumentsInfo(UserDocsClient objDocumentsInfo, FormCollection form)
        {
            DateTime cd = DateTime.Parse(form["hdnCD1"]);


            if (ModelState.IsValid)
            {
                List<UserDocsClient> debDocumentsInfoList = TempDataManager.LoadTempData(this, "debOrdDocumentsInfo") as List<UserDocsClient>;
                var oldUserDocument = debDocumentsInfoList.Where(d => d.Created == cd).FirstOrDefault();
                if (oldUserDocument == null || debDocumentsInfoList == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                var clone = StreamingManager.Clone(oldUserDocument) as UserDocsClient;
                var values = JsonConvert.SerializeObject(form.AllKeys.ToDictionary(k => k, k => form[k]));
                JsonConvert.PopulateObject(values, clone);
                clone.Created = oldUserDocument.Created;
                clone._Created = oldUserDocument._Created;
                var err = await DataQuery.Update(oldUserDocument, clone);
                if (err == ErrorCodes.Succes)
                {
                    int indrow = debDocumentsInfoList.IndexOf(oldUserDocument);
                    debDocumentsInfoList.RemoveAt(indrow);
                    debDocumentsInfoList.Insert(indrow, clone);
                    TempDataManager.SaveTempData(this, "debOrdDocumentsInfo", debDocumentsInfoList);

                    return Json(new { success = true, message = string.Format(Localization.lookup("SavedOBJ"), string.Empty) });
                }
                else
                    return await getJsonError(err);
            }
            else
            {
                return getModelStateError();
            }
        }

        public ActionResult DeleteDebOrdDocumentsInfo(DateTime? cd = null)
        {

            ViewBag.toolChosen = "Delete";
            ViewBag.dateCreated = cd;
            List<UserDocsClient> debDocumentsInfoList = TempDataManager.LoadTempData(this, "debOrdDocumentsInfo") as List<UserDocsClient>;
            var dbUserDocument = debDocumentsInfoList.Where(d => d.Created == cd).FirstOrDefault();
            if (dbUserDocument != null)
            {
                return View("DebOrderDocuments_Model", dbUserDocument);
            }
            return RedirectToAction("Login", "Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteDebOrdDocumentsInfo(FormCollection form)
        {
            DateTime cd = DateTime.Parse(form["hdnCD1"]);

            List<UserDocsClient> debDocumentsInfoList = TempDataManager.LoadTempData(this, "debOrdDocumentsInfo") as List<UserDocsClient>;
            var dbUserDocument = debDocumentsInfoList.Where(d => d.Created == cd).FirstOrDefault();

            if (dbUserDocument != null)
            {
                var err = await DataQuery.Delete(dbUserDocument);
                if (err == ErrorCodes.Succes)
                {
                    debDocumentsInfoList.Remove(dbUserDocument);
                    TempDataManager.SaveTempData(this, "debOrdDocumentsInfo", debDocumentsInfoList);
                    return Json(new { success = true, message = Localization.lookup("Deleted"), string.Empty });
                }
                else
                {
                    return await getJsonError(err);
                }
            }
            else
            {
                return RedirectToAction("Login", "Login");
            }
        }


        #endregion

        #region Debitor Statement

        static List<DebitorStatement> statementsClient;
        //IEnumerable<PropValuePair> debtorFilterValues;
        //Task<SQLCache> accountCacheTask;

        public ActionResult Statement()
        {


            clearDebStatement();
            return View();
        }

        /// <summary>
        /// Clear the list of Debitor Statements.
        /// </summary>
        private void clearDebStatement()
        {
            if (statementsClient != null && statementsClient.Count > 0)
            {
                statementsClient.Clear();
            }
        }

        [HttpGet]
        public ActionResult GenerateStatements(string fromAccount, string toAccount, string dateFrom, string dateTo, string isForAsce, string isForSkipEmp, string isForOnlyOpen)
        {
            ViewBag.CompanyDetatil = si.CurrentCompany;
            ViewBag.FromAccount = fromAccount;
            ViewBag.ToAccount = toAccount;
            ViewBag.DateFrom = dateFrom;
            ViewBag.DateTo = dateTo;
            ViewBag.IsForAsce = isForAsce;
            ViewBag.IsForSkipEmp = isForSkipEmp;
            ViewBag.IsForOpneOnly = isForOnlyOpen;
            return View("_PartialDebStatement");
        }

        [HttpGet]
        public async Task<ActionResult> LoadDebStatements(DataSourceLoadOptions loadOptions, string isGridRefreshed, string fromAccount, string toAccount, string dateFrom, string dateTo, string isForAsce, string isForSkipEmp, string isForOnlyOpen)
        {
            //clear the DebitorMasterList
            clearDebStatement();



            SQLCache accountCache = si.BaseAPI.CompanyEntity.GetCache(typeof(Uniconta.DataModel.Debtor));
            if (accountCache == null)
                accountCache = await si.BaseAPI.CompanyEntity.LoadCache(typeof(Uniconta.DataModel.Debtor), si.BaseAPI).ConfigureAwait(false);

            var Pref = si.BaseAPI.session.Preference;
            var isAscending = Pref.Debtor_isAscending = (isForAsce == "true" ? true : false);
            var skipBlank = Pref.Debtor_skipBlank = (isForSkipEmp == "true" ? true : false);
            var onlyOpen = Pref.Debtor_OnlyOpen = (isForOnlyOpen == "true" ? true : false);
            DateTime fromDate = Uniconta.Common.Utility.NumberConvert.ToDate(dateFrom);
            DateTime toDate = Uniconta.Common.Utility.NumberConvert.ToDate(dateTo);

            statementsClient = new List<DebitorStatement>();
            var transAPI = new ReportAPI(si.BaseAPI);

            //var listTrans = (DebtorTransClientTotalStmnt[])await transAPI.GetTransWithPrimo(new DebtorTransClientTotalStmnt(), fromDate, toDate, fromAccount, toAccount, onlyOpen, null, debtorFilterValues);
            var listTrans = (DebtorTransClientTotalStmnt[])await transAPI.GetTransWithPrimo(new DebtorTransClientTotalStmnt(), fromDate, toDate, fromAccount, toAccount, onlyOpen, null, null);
            if (listTrans != null)
            {
                //var transCache = accountCacheTask;
                //if (transCache != null)
                //    accountCache = await transCache;

                string currentItem = string.Empty;
                DebitorStatement masterDbStatement = null;
                List<DebtorTransClientTotalStmnt> dbTransClientChildList = null;
                double SumAmount = 0d, SumAmountCur = 0d;

                foreach (var trans in listTrans)
                {
                    if (trans._Account != currentItem)
                    {
                        currentItem = trans._Account;
                        if (masterDbStatement != null)
                        {
                            if (!skipBlank || SumAmount != 0 || dbTransClientChildList.Count > 1)
                                statementsClient.Add(masterDbStatement);
                        }
                        masterDbStatement = new DebitorStatement((Debtor)accountCache.Get(currentItem));
                        dbTransClientChildList = new List<DebtorTransClientTotalStmnt>();
                        masterDbStatement.ChildRecords = dbTransClientChildList;
                        SumAmount = SumAmountCur = 0d;
                        if (trans._Text == null && trans._Primo)
                            trans._Text = Uniconta.ClientTools.Localization.lookup("Primo");
                    }
                    SumAmount += trans._Amount;
                    trans._SumAmount = SumAmount;
                    masterDbStatement._SumAmount = SumAmount;
                    SumAmountCur += trans._AmountCur;
                    trans._SumAmountCur = SumAmountCur;
                    masterDbStatement._SumAmountCur = SumAmountCur;

                    if (isAscending)
                        dbTransClientChildList.Add(trans);
                    else
                        dbTransClientChildList.Insert(0, trans);
                }

                if (masterDbStatement != null)
                {
                    if (!skipBlank || SumAmount != 0 || dbTransClientChildList.Count > 1)
                        statementsClient.Add(masterDbStatement);
                }

                //if (statementsClient.Any())
                //{
                //    dgDebtorTrans.ItemsSource = null;
                //    dgDebtorTrans.ItemsSource = statementsClient;
                //}
                //dgDebtorTrans.Visibility = Visibility.Visible;                
            }
            return Content(JsonConvert.SerializeObject(DataSourceLoader.Load(statementsClient, loadOptions), new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), "application/json");
        }

        [HttpGet]
        public ActionResult LoadDebChieldStatements(DataSourceLoadOptions loadOptions, string id)
        {
            List<DebtorTransClientTotalStmnt> tlst = new List<DebtorTransClientTotalStmnt>();
            var data = statementsClient.Where(d => d.AccountNumber == id).FirstOrDefault();
            tlst = data.ChildRecords;
            return Content(JsonConvert.SerializeObject(DataSourceLoader.Load(tlst, loadOptions), new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), "application/json");
        }
        #endregion

        #region Debitor Offers
        public ActionResult DebtorOffers()
        {
            TempData.Clear();
            ViewBag.CompanyDetatil = si.CurrentCompany;
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> LoadDebtorOffers(DataSourceLoadOptions loadOptions, string IsGridRefreshed)
        {
            bool IsRefreshed = false;
            if (!String.IsNullOrEmpty(IsGridRefreshed))
            {
                IsRefreshed = Convert.ToBoolean(IsGridRefreshed);
            }
            List<DebtorOfferPortal> DebtorOfferList = TempDataManager.LoadTempData(this, "debOffers") as List<DebtorOfferPortal>;
            if (IsRefreshed || DebtorOfferList == null || DebtorOfferList.Count == 0)
            {
                var res = await FilterGrid(typeof(DebtorOfferPortal));
                DebtorOfferList = GetList(res);
                TempDataManager.SaveTempData(this, "debOffers", DebtorOfferList);
            }
            return Content(JsonConvert.SerializeObject(DataSourceLoader.Load(DebtorOfferList, loadOptions), new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), "application/json");
        }

        [HttpGet]
        public ActionResult DebOfferDetails(int id = 0)
        {
            ViewBag.toolChosen = "DebOfferDetail";
            List<DebtorOfferPortal> DebtorOfferList = TempDataManager.LoadTempData(this, "debOffers") as List<DebtorOfferPortal>;
            var debtorOffer = DebtorOfferList.Where(d => d.RowId == id).FirstOrDefault();
            if (debtorOffer != null)
            {
                ViewBag.quotationNumber = debtorOffer.OfferNumber;
                debtorOffer.CompanyEntity = si.CurrentCompany;
                GetCountries();
                GetCurrencies();
                return View("DebtorOffers_Model", debtorOffer);
            }
            return RedirectToAction("Login", "Login");
        }

        [HttpGet]
        public ActionResult CreateOffer()
        {
            ViewBag.toolChosen = "CreateDebOffer";
            var debOfferClient = new DebtorOfferPortal();
            debOfferClient.CompanyEntity = si.CurrentCompany;
            debOfferClient.SetMaster(debOfferClient.CompanyEntity);
            GetCountries();
            GetCurrencies();
            return PartialView("DebtorOffers_Model", debOfferClient);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateOffer(DebtorOfferPortal newDebOfferData, FormCollection frm)
        {
            var userAction = frm.Get("hdnUserAction");

            List<DebtorOfferPortal> DebtorOfferList = TempDataManager.LoadTempData(this, "debOffers") as List<DebtorOfferPortal>;
            if (ModelState.IsValid)
            {
                newDebOfferData.SetMaster(si.CurrentCompany);

                var err = await DataQuery.Insert(newDebOfferData);
                if (err == ErrorCodes.Succes)
                {
                    DebtorOfferList.Add(newDebOfferData);
                    TempDataManager.SaveTempData(this, "debOffers", DebtorOfferList);

                    if (userAction.ToLower() == "saveandgotoline")
                    {
                        return Json(new { saveNGoToLine = true, message = string.Format(Localization.lookup("SavedOBJ"), string.Empty), rowId = newDebOfferData.RowId });
                    }
                    return Json(new { success = true, message = string.Format(Localization.lookup("SavedOBJ"), string.Empty), rowid = newDebOfferData.RowId });
                }
                else
                {
                    return await getJsonError(err);
                }
            }
            else
            {
                return getModelStateError();
            }
        }

        [HttpGet]
        public ActionResult EditDebOffer(int id = 0)
        {
            ViewBag.toolChosen = "EditDebOffer";
            DebtorOfferPortal debtorOffer;
            List<DebtorOfferPortal> DebtorOfferList = TempDataManager.LoadTempData(this, "debOffers") as List<DebtorOfferPortal>;

            debtorOffer = DebtorOfferList.Where(d => d.RowId == id).FirstOrDefault();
            if (debtorOffer != null)
            {
                ViewBag.quotationNumber = debtorOffer.OfferNumber;
                debtorOffer.CompanyEntity = si.CurrentCompany;
                GetCountries();
                GetCurrencies();
                return View("DebtorOffers_Model", debtorOffer);
            }
            return RedirectToAction("Login", "Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditDebOffer(int id, DebtorOfferPortal newDebOfferData, FormCollection frm)
        {
            var userAction = frm.Get("hdnUserAction");

            if (ModelState.IsValid)
            {
                List<DebtorOfferPortal> DebtorOfferList = TempDataManager.LoadTempData(this, "debOffers") as List<DebtorOfferPortal>;
                DebtorOfferPortal oldDebtorOffer = new DebtorOfferPortal();
                if (DebtorOfferList != null)
                {
                    oldDebtorOffer = DebtorOfferList.Where(d => d.RowId == id).FirstOrDefault();
                }
                if (oldDebtorOffer == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                newDebOfferData.PricesInclVat = CheckBoolean(newDebOfferData.PricesInclVat.ToString().ToLower());
                newDebOfferData.SetMaster(si.CurrentCompany);

                var err = await DataQuery.Update(oldDebtorOffer, newDebOfferData);

                if (err == ErrorCodes.Succes)
                {
                    int indrow = DebtorOfferList.IndexOf(oldDebtorOffer);
                    DebtorOfferList.RemoveAt(indrow);
                    DebtorOfferList.Insert(indrow, newDebOfferData);
                    TempDataManager.SaveTempData(this, "debOffers", DebtorOfferList);
                    if (userAction.ToLower() == "saveandgotoline")
                    {
                        return Json(new { saveNGoToLine = true, message = string.Format(Localization.lookup("SavedOBJ"), string.Empty), rowId = newDebOfferData.RowId });
                    }
                    return Json(new { success = true, message = string.Format(Localization.lookup("SavedOBJ"), string.Empty) });
                }
                else
                {
                    return await getJsonError(err);
                }
            }
            else
                return getModelStateError();
        }

        [HttpGet]
        public ActionResult DeleteDebOffer(int id = 0)
        {
            ViewBag.toolChosen = "DeleteDebOffer";
            DebtorOfferPortal debtorOffer;

            List<DebtorOfferPortal> DebtorOfferList = TempDataManager.LoadTempData(this, "debOffers") as List<DebtorOfferPortal>;
            if (DebtorOfferList == null)
            {
                return RedirectToAction("Login", "Login");
            }
            debtorOffer = DebtorOfferList.Where(d => d.RowId == id).FirstOrDefault();
            if (debtorOffer != null)
            {
                ViewBag.quotationNumber = debtorOffer.OfferNumber;
                debtorOffer.CompanyEntity = si.CurrentCompany;
                GetCountries();
                GetCurrencies();
                return View("DebtorOffers_Model", debtorOffer);
            }
            return RedirectToAction("Login", "Login");
        }

        [HttpPost, ActionName("DeleteDebOffer")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteDebOfferConfirmed(int id)
        {
            List<DebtorOfferPortal> DebtorOfferList = TempDataManager.LoadTempData(this, "debOffers") as List<DebtorOfferPortal>;
            var debtor = DebtorOfferList.Where(d => d.RowId == id).FirstOrDefault();

            if (debtor != null)
            {
                var err = await DataQuery.Delete(debtor);
                if (err == ErrorCodes.Succes)
                {
                    DebtorOfferList.Remove(debtor);
                    TempDataManager.SaveTempData(this, "debOffers", DebtorOfferList);
                    return Json(new { success = true, message = Localization.lookup("Deleted"), rowid = id });
                }
                else
                {
                    return await getJsonError(err);
                }
            }
            return RedirectToAction("Login", "Login");
        }
        #endregion

        #region Debtor Offer Lines
        [HttpGet]
        public async Task<ActionResult> DebtorOfferLines(int rowId = 0)
        {
            TempDataManager.RemoveTempData(this, "dbOfferLines");
            TempDataManager.RemoveTempData(this, "dbOpenOffer");
            ViewBag.CompanyDetatil = si.CurrentCompany;
            GetUnits();

            if (rowId != 0)
            {
                List<DebtorOfferPortal> DebtorOfferList = TempDataManager.LoadTempData(this, "debOffers") as List<DebtorOfferPortal>;
                if (DebtorOfferList == null || DebtorOfferList.Count == 0)
                {
                    var res = await FilterGrid(typeof(DebtorOfferPortal));
                    DebtorOfferList = GetList(res);
                    TempDataManager.SaveTempData(this, "debOffers", DebtorOfferList);
                }
                var debtorOffer = DebtorOfferList.Where(d => d.RowId == rowId).FirstOrDefault();
                ViewBag.AccName = debtorOffer.Name;
                ViewBag.QtNumber = debtorOffer.OfferNumber;
                ViewBag.RowId = debtorOffer.RowId;
                TempDataManager.SaveTempData(this, "dbOpenOffer", debtorOffer);
                PriceLookup = new Uniconta.API.DebtorCreditor.FindPrices(debtorOffer, si.BaseAPI);
                TempDataManager.SaveTempData(this, "priceLookup", PriceLookup);
            }
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> LoadDebtorOfferLines(DataSourceLoadOptions loadOptions, string IsGridRefreshed, int rowId = 0)
        {
            TempDataManager.RemoveTempData(this, "dbOpenOfferLine");
            Session["curOfferRowId"] = null;

            bool IsRefreshed = false;
            if (!String.IsNullOrEmpty(IsGridRefreshed))
            {
                IsRefreshed = Convert.ToBoolean(IsGridRefreshed);
            }

            List<DebtorOfferPortal> DebtorOfferList = TempDataManager.LoadTempData(this, "debOffers") as List<DebtorOfferPortal>;
            var openDebtorOffer = DebtorOfferList.Where(d => d.RowId == rowId).FirstOrDefault();

            //For set master during insertion of new line.
            TempDataManager.SaveTempData(this, "dbOpenOffer", openDebtorOffer);

            List<DebtorOfferLinePortal> DebtorOfferLineList = TempDataManager.LoadTempData(this, "dbOfferLines") as List<DebtorOfferLinePortal>;
            if (IsRefreshed || (openDebtorOffer != null && DebtorOfferLineList == null) || (openDebtorOffer != null && DebtorOfferLineList.Count == 0))
            {
                var res = await FilterGrid(typeof(DebtorOfferLinePortal), new List<UnicontaBaseEntity>() { openDebtorOffer });
                List<DebtorOfferLinePortal> resultList = res.OfType<DebtorOfferLinePortal>().ToList();
                TempDataManager.SaveTempData(this, "dbOfferLines", resultList);

                return Content(JsonConvert.SerializeObject(DataSourceLoader.Load(resultList, loadOptions), new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), "application/json");
            }
            else if (openDebtorOffer != null && DebtorOfferLineList != null)
            {
                return Content(JsonConvert.SerializeObject(DataSourceLoader.Load(DebtorOfferLineList, loadOptions), new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), "application/json");
            }
            return HttpNotFound();
        }

        [HttpPut]
        public async Task<ActionResult> UpdateOfferLine(FormCollection form)
        {
            var values = form.Get("values");
            var key = Convert.ToInt32(form.Get("key"));

            // Converting the JSON into a Sub Delivery Order object that belongs to the model
            List<DebtorOfferLinePortal> DebtorOfferLineList = TempDataManager.LoadTempData(this, "dbOfferLines") as List<DebtorOfferLinePortal>;
            var oldDeptorOfferLine = DebtorOfferLineList.Where(d => d.RowId == key).FirstOrDefault();
            if (oldDeptorOfferLine == null)
            {
                return RedirectToAction("Login", "Login");
            }
            if (ModelState.IsValid)
            {
                var clone = StreamingManager.Clone(oldDeptorOfferLine) as DebtorOfferLinePortal;
                JsonConvert.PopulateObject(values, clone);
                var err = await DataQuery.Update(oldDeptorOfferLine, clone);
                if (err == ErrorCodes.Succes)
                {
                    int indrow = DebtorOfferLineList.IndexOf(oldDeptorOfferLine);
                    DebtorOfferLineList.RemoveAt(indrow);
                    DebtorOfferLineList.Insert(indrow, clone);
                    TempDataManager.SaveTempData(this, "dbOfferLines", DebtorOfferLineList);
                    return Json(new { success = true, message = string.Format(Localization.lookup("SavedOBJ"), string.Empty) });
                }
                else
                    GetUnits();
                return await getJsonError(err);
            }
            else
            {
                return getModelStateError();
            }
        }

        [HttpPost]
        public async Task<ActionResult> InsertOfferLine(FormCollection form)
        {
            SQLCache debtorOffers = si.CurrentCompany.GetCache(typeof(DebtorOffer));
            if (debtorOffers == null)
                debtorOffers = await si.CurrentCompany.LoadCache(typeof(DebtorOffer), si.BaseAPI).ConfigureAwait(true);

            List<DebtorOfferLinePortal> DebtorOfferLineList = TempDataManager.LoadTempData(this, "dbOfferLines") as List<DebtorOfferLinePortal>;
            DebtorOfferLinePortal debtorOfferLine = new DebtorOfferLinePortal();
            //To Fetch the current open order
            DebtorOfferPortal OpenDeptorOffer = TempDataManager.LoadTempData(this, "dbOpenOffer") as DebtorOfferPortal;

            if (ModelState.IsValid)
            {
                var values = form.Get("values");
                JsonConvert.PopulateObject(values, debtorOfferLine);
                debtorOfferLine.SetMaster(OpenDeptorOffer);
                var err = await DataQuery.Insert(debtorOfferLine);
                if (err == ErrorCodes.Succes)
                {
                    DebtorOfferLineList.Add(debtorOfferLine);
                    TempDataManager.SaveTempData(this, "dbOfferLines", DebtorOfferLineList);
                    return Json(new { Insertsuccess = true, message = string.Format(Localization.lookup("SavedOBJ"), string.Empty), rowid = debtorOfferLine.RowId });
                }
                else
                {
                    GetUnits();
                    return await getJsonError(err);
                }
            }
            else
            {
                return getModelStateError();
            }
        }

        //[HttpPost]
        public async Task<ActionResult> DeleteOfferLine(FormCollection form)
        {
            var key = Convert.ToInt32(form.Get("key"));

            List<DebtorOfferLinePortal> DebtorOfferLineList = TempDataManager.LoadTempData(this, "dbOfferLines") as List<DebtorOfferLinePortal>;
            var deptorOrderlineToDelete = DebtorOfferLineList.Where(d => d.RowId == key).FirstOrDefault();
            if (deptorOrderlineToDelete != null)
            {
                var err = await DataQuery.Delete(deptorOrderlineToDelete);
                if (err == ErrorCodes.Succes)
                {
                    DebtorOfferLineList.Remove(deptorOrderlineToDelete);
                    TempDataManager.SaveTempData(this, "dbOfferLines", DebtorOfferLineList);
                    return Json(new { DeleteSuccess = true, message = Localization.lookup("Deleted"), rowid = key });
                }
                else
                {
                    return await getJsonError(err);
                }
            }

            return null;
        }


        public async Task<ActionResult> setDebOfferFields(string value, string propertyName, int id = 0)
        {
            var api = si.BaseAPI;
            DebtorOfferLinePortal res = new DebtorOfferLinePortal();
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Error = (serializer, err) => err.ErrorContext.Handled = true;

            try
            {
                string str = string.Empty;
                if (Session["curOfferRowId"] != null)
                {
                    str = Session["curOfferRowId"].ToString();
                }

                DebtorOfferLinePortal OpenDBOfferLine = TempDataManager.LoadTempData(this, "dbOpenOfferLine") as DebtorOfferLinePortal;

                if (OpenDBOfferLine != null && str != id.ToString())
                {
                    TempDataManager.RemoveTempData(this, "dbOpenOfferLine");
                    OpenDBOfferLine = TempDataManager.LoadTempData(this, "dbOpenOfferLine") as DebtorOfferLinePortal;
                }

                if (id != 0 && OpenDBOfferLine == null && propertyName != "Item")
                {
                    Session["curOfferRowId"] = id;
                    List<DebtorOfferLinePortal> DebtorOrderLinelist = TempDataManager.LoadTempData(this, "dbOfferLines") as List<DebtorOfferLinePortal>;
                    var cloneVal = DebtorOrderLinelist.Where(d => d.RowId == id).FirstOrDefault();
                    res = StreamingManager.Clone(cloneVal) as DebtorOfferLinePortal;
                }
                else if ((id == 0 && OpenDBOfferLine == null) || propertyName == "Item")
                {
                    Session["curOfferRowId"] = id;
                    DebtorOfferPortal OpenDeptorOffer = TempDataManager.LoadTempData(this, "dbOpenOffer") as DebtorOfferPortal;
                    var serializedVal = JsonConvert.SerializeObject(OpenDeptorOffer, settings);
                    //Note:-Account and company id missed here, have to manage it by setting account type.
                    JsonConvert.PopulateObject(serializedVal, res, settings);
                }
                else
                {
                    res = OpenDBOfferLine;
                }

                if (propertyName == "Item")
                {
                    res._Variant1 = null;
                    res._Variant2 = null;
                    res._Qty = 0;
                    await setOfferItem(value, res);
                }
                else if (propertyName == "Qty")
                {
                    await ManagePriceLookUpTask();

                    res._Qty = Convert.ToDouble(value);
                    if (this.PriceLookup != null && this.PriceLookup.UseCustomerPrices)
                        await this.PriceLookup.GetCustomerPrice(res, false);
                }
                else if (propertyName == "Subtotal" || propertyName == "Total")
                {
                    res._AmountEntered = res.Total = Convert.ToDouble(value);
                }
                else if (propertyName == "EAN")
                {
                    SQLCache items = si.CurrentCompany.GetCache(typeof(InvItem)) ?? await si.CurrentCompany.LoadCache(typeof(InvItem), api).ConfigureAwait(false);
                    res._EAN = value;
                    await FindOnEAN(res, items, api);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            TempDataManager.SaveTempData(this, "dbOpenOfferLine", res);
            var jsonResult = Json(new { result = JsonConvert.SerializeObject(res, JsonSettings) }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        async Task setOfferItem(string value, DebtorOfferLinePortal res)
        {
            try
            {
                SQLCache items;
                var api = si.BaseAPI;
                items = si.CurrentCompany.GetCache(typeof(InvItem)) ?? await si.CurrentCompany.LoadCache(typeof(InvItem), api).ConfigureAwait(false);
                var selectedItem = (InvItem)items.Get(value);

                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.Error = (serializer, err) => err.ErrorContext.Handled = true;
                var values = JsonConvert.SerializeObject(selectedItem, settings);
                JsonConvert.PopulateObject(values, res, settings);

                if (items != null)
                {
                    if (selectedItem != null)
                    {
                        if (selectedItem._AlternativeItem != null && selectedItem._UseAlternative == UseAlternativeItem.Always)
                        {
                            var altItem = (InvItem)items.Get(selectedItem._AlternativeItem);
                            if (altItem != null && altItem._AlternativeItem == null)
                            {
                                res._Item = selectedItem._AlternativeItem;
                                return;
                            }
                        }
                        if (selectedItem._SalesQty != 0d)
                            res._Qty = selectedItem._SalesQty;
                        else if (api.CompanyEntity._OrderLineOne)
                            res._Qty = 1d;
                        res.SetItemValues(selectedItem);

                        await ManagePriceLookUpTask();
                        Task t = this.PriceLookup.SetPriceFromItem(res, selectedItem);
                        if (t != null)
                            await t;

                        if (selectedItem._StandardVariant != res.standardVariant)
                        {
                            res._Variant1 = null;
                            res._Variant2 = null;
                            res.variant2Source = null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region Debtor Offer User Notes
        [HttpGet]
        public ActionResult DebOfferUserNotes(int rowId = 0)
        {
            ViewBag.RowId = rowId;

            TempDataManager.RemoveTempData(this, "debOfferUserNotes");
            TempDataManager.RemoveTempData(this, "debOfferMaster");
            if (rowId != 0)
            {
                List<DebtorOfferPortal> DebtorOfferList = TempDataManager.LoadTempData(this, "debOffers") as List<DebtorOfferPortal>;
                var debOfferById = DebtorOfferList.Where(s => s.RowId == rowId).FirstOrDefault();
                ViewBag.AccountNumber = debOfferById.Account;
                ViewBag.AccountName = debOfferById.Name;
            }
            return View("DebOfferNotesInfo");
        }

        [HttpGet]
        public async Task<ActionResult> LoadDebOfferUserNotes(DataSourceLoadOptions loadOptions, string IsGridRefreshed, int rowId = 0)
        {
            bool IsRefreshed = false;
            if (!String.IsNullOrEmpty(IsGridRefreshed))
            {
                IsRefreshed = Convert.ToBoolean(IsGridRefreshed);
            }

            List<DebtorOfferPortal> DebtorOfferList = TempDataManager.LoadTempData(this, "debOffers") as List<DebtorOfferPortal>;
            DebtorOfferPortal debOfferById = DebtorOfferList.Where(d => d.RowId == rowId).FirstOrDefault();
            TempDataManager.SaveTempData(this, "debOfferMaster", debOfferById);
            List<UserNotesClient> debOfferUserNotesList = TempDataManager.LoadTempData(this, "debOfferUserNotes") as List<UserNotesClient>;

            if (IsRefreshed || (debOfferById != null && debOfferUserNotesList == null) || (debOfferById != null && debOfferUserNotesList.Count == 0))
            {
                var res = await FilterGrid(typeof(UserNotesClient), new List<UnicontaBaseEntity>() { debOfferById });
                if (res != null)
                {
                    debOfferUserNotesList = GetList(res, typeof(UserNotesClient));
                    TempDataManager.SaveTempData(this, "debOfferUserNotes", debOfferUserNotesList);
                    return Content(JsonConvert.SerializeObject(DataSourceLoader.Load(debOfferUserNotesList, loadOptions), new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), "application/json");
                }
            }
            else if (debOfferById != null && debOfferUserNotesList != null)
            {
                return Content(JsonConvert.SerializeObject(DataSourceLoader.Load(debOfferUserNotesList, loadOptions), new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), "application/json");
            }
            return HttpNotFound();
        }

        [HttpGet]
        public ActionResult CreateDebOfferUserNotes()
        {
            ViewBag.toolChosen = "CreateOfferNote";
            var debUserNote = new UserNotesClient();

            debUserNote.Created = DateTime.Now;
            return View("DebOfferNotesInfo_Model", debUserNote);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateDebOfferUserNotes(UserNotesClient newDebtorNoteData)
        {
            List<UserNotesClient> debOfferUserNotesList = TempDataManager.LoadTempData(this, "debOfferUserNotes") as List<UserNotesClient>;
            DebtorOfferPortal debOffer = TempDataManager.LoadTempData(this, "debOfferMaster") as DebtorOfferPortal;
            if (ModelState.IsValid)
            {

                newDebtorNoteData.SetMaster(debOffer);
                var err = await DataQuery.Insert(newDebtorNoteData);
                if (err == ErrorCodes.Succes)
                {
                    debOfferUserNotesList.Add(newDebtorNoteData);
                    TempDataManager.SaveTempData(this, "debOfferUserNotes", debOfferUserNotesList);
                    return Json(new { success = true, message = string.Format(Localization.lookup("SavedOBJ"), string.Empty) });
                }
                else
                    return await getJsonError(err);
            }
            else
            {
                return getModelStateError();
            }

        }

        public ActionResult EditDebOfferUserNotes(DateTime? cd = null)
        {
            ViewBag.toolChosen = "EditOfferNote";
            ViewBag.dateCreated = cd;
            List<UserNotesClient> debUserNotesList = TempDataManager.LoadTempData(this, "debOfferUserNotes") as List<UserNotesClient>;
            var objUserNote = debUserNotesList.Where(d => d.Created == cd).FirstOrDefault();
            if (objUserNote != null)
            {
                return View("DebOfferNotesInfo_Model", objUserNote);
            }
            return RedirectToAction("Login", "Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditDebOfferUserNotes(UserNotesClient objUserNotes, FormCollection form)
        {
            DateTime cd = DateTime.Parse(form["hdnCD1"]);

            if (ModelState.IsValid)
            {
                List<UserNotesClient> lstDebUserNotes = TempDataManager.LoadTempData(this, "debOfferUserNotes") as List<UserNotesClient>;
                var oldDebUserNote = lstDebUserNotes.Where(d => d.Created == cd).FirstOrDefault();
                if (oldDebUserNote == null || lstDebUserNotes == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                var clone = StreamingManager.Clone(oldDebUserNote) as UserNotesClient;
                var values = JsonConvert.SerializeObject(form.AllKeys.ToDictionary(k => k, k => form[k]));
                JsonConvert.PopulateObject(values, clone);
                clone.Created = oldDebUserNote.Created;
                clone._Created = oldDebUserNote._Created;
                var err = await DataQuery.Update(oldDebUserNote, clone);

                if (err == ErrorCodes.Succes)
                {
                    int indrow = lstDebUserNotes.IndexOf(oldDebUserNote);
                    lstDebUserNotes.RemoveAt(indrow);
                    lstDebUserNotes.Insert(indrow, clone);
                    TempDataManager.SaveTempData(this, "debOfferUserNotes", lstDebUserNotes);
                    return Json(new { success = true, message = string.Format(Localization.lookup("SavedOBJ"), string.Empty) });
                }
                else
                {
                    return await getJsonError(err);
                }
            }
            else
                return getModelStateError();
        }

        public ActionResult DeleteDebOfferUserNotes(DateTime? cd = null)
        {
            ViewBag.toolChosen = "DeleteOfferNote";
            ViewBag.dateCreated = cd;
            List<UserNotesClient> lstDebUserNotes = TempDataManager.LoadTempData(this, "debOfferUserNotes") as List<UserNotesClient>;
            var debUserNote = lstDebUserNotes.Where(d => d.Created == cd).FirstOrDefault();
            if (debUserNote != null)
            {
                return View("DebOfferNotesInfo_Model", debUserNote);
            }
            return RedirectToAction("Login", "Login");
        }

        [HttpPost, ActionName("DeleteDebOfferUserNotes")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteOfferUserNotes(FormCollection form)
        {
            DateTime cd = DateTime.Parse(form["hdnCD1"]);
            List<UserNotesClient> lstDebUserNotes = TempDataManager.LoadTempData(this, "debOfferUserNotes") as List<UserNotesClient>;
            var debUserNote = lstDebUserNotes.Where(d => d.Created == cd).FirstOrDefault();

            if (debUserNote != null)
            {
                var err = await DataQuery.Delete(debUserNote);
                if (err == ErrorCodes.Succes)
                {
                    lstDebUserNotes.Remove(debUserNote);
                    TempDataManager.SaveTempData(this, "debOfferUserNotes", lstDebUserNotes);
                    return Json(new { success = true, message = Localization.lookup("Deleted"), string.Empty });
                }
                else
                {
                    return await getJsonError(err);
                }
            }
            else
            {
                return RedirectToAction("Login", "Login");
            }
        }


        #endregion

        #region Debtor Offer Document
        [HttpGet]
        public ActionResult DebOfferDocuments(int rowId = 0)
        {
            ViewBag.RowId = rowId;

            //clear ordersLines
            TempDataManager.RemoveTempData(this, "debOfferDocumentsInfo");
            TempDataManager.RemoveTempData(this, "debOfferAccountMaster");
            if (rowId != 0)
            {
                List<DebtorOfferPortal> DebtorOfferList = TempDataManager.LoadTempData(this, "debOffers") as List<DebtorOfferPortal>;
                var debOfferById = DebtorOfferList.Where(s => s.RowId == rowId).FirstOrDefault();
                ViewBag.AccountNumber = debOfferById.Account;
                ViewBag.AccountName = debOfferById.Name;
            }
            return View("DebOfferDocumentsInfo");
        }

        [HttpGet]
        public async Task<ActionResult> LoadDebOfferDocuments(DataSourceLoadOptions loadOptions, string IsGridRefreshed, int rowId = 0)
        {
            bool IsRefreshed = false;
            if (!String.IsNullOrEmpty(IsGridRefreshed))
            {
                IsRefreshed = Convert.ToBoolean(IsGridRefreshed);
            }

            List<DebtorOfferPortal> DebtorOfferList = TempDataManager.LoadTempData(this, "debOffers") as List<DebtorOfferPortal>;
            DebtorOfferPortal debOfferById = DebtorOfferList.Where(d => d.RowId == rowId).FirstOrDefault();
            TempDataManager.SaveTempData(this, "debOfferAccountMaster", debOfferById);
            List<UserDocsClient> debDocumentsList = TempDataManager.LoadTempData(this, "debOfferDocumentsInfo") as List<UserDocsClient>;

            if (IsRefreshed || (debOfferById != null && debDocumentsList == null) || (debOfferById != null && debDocumentsList.Count == 0))
            {
                var res = await FilterGrid(typeof(UserDocsClient), new List<UnicontaBaseEntity>() { debOfferById });
                if (res != null)
                {
                    debDocumentsList = GetList(res, typeof(UserDocsClient));
                    TempDataManager.SaveTempData(this, "debOfferDocumentsInfo", debDocumentsList);
                    return Content(JsonConvert.SerializeObject(DataSourceLoader.Load(debDocumentsList, loadOptions), new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), "application/json");
                }
            }
            else if (debOfferById != null && debDocumentsList != null)
            {
                return Content(JsonConvert.SerializeObject(DataSourceLoader.Load(debDocumentsList, loadOptions), new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), "application/json");
            }
            return RedirectToAction("Login", "Login");
        }

        [HttpGet]
        public ActionResult CreateDebOfferDocuments()
        {
            ViewBag.toolChosen = "CreateOfferDoc";

            var debUserDocument = new UserDocsClient();
            debUserDocument.Created = DateTime.Now;
            return View("DebOfferDocuments_Model", debUserDocument);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateDebOfferDocuments(HttpPostedFileBase files, FormCollection form)
        {

            List<UserDocsClient> debDocumentsInfoList = TempDataManager.LoadTempData(this, "debOfferDocumentsInfo") as List<UserDocsClient>;
            DebtorOfferPortal debOffer = TempDataManager.LoadTempData(this, "debOfferAccountMaster") as DebtorOfferPortal;
            UserDocsClient objUserDocument = new UserDocsClient();
            objUserDocument.SetMaster(debOffer);
            objUserDocument.Text = form["Text"];
            objUserDocument.Group = form["Group"];
            objUserDocument.DocumentType = DocumentConvert.GetDocumentType(Path.GetExtension(files.FileName));
            if (ModelState.IsValid)
            {
                MemoryStream target = new MemoryStream();
                files.InputStream.CopyTo(target);
                byte[] fileBytes = target.ToArray();
                if (fileBytes.Length <= TableAddOnData.MaxDocSize && fileBytes.Length > 0)
                {
                    objUserDocument.UserDocument = fileBytes;
                }
                var err = await DataQuery.Insert(objUserDocument);
                if (err == ErrorCodes.Succes)
                {
                    debDocumentsInfoList.Add(objUserDocument);
                    TempDataManager.SaveTempData(this, "debOfferDocumentsInfo", debDocumentsInfoList);
                    return Json(new { success = true, message = string.Format(Localization.lookup("SavedOBJ"), string.Empty) });
                }
                else
                    return await getJsonError(err);
            }
            else
            {
                return getModelStateError();
            }
        }

        public ActionResult EditDebOfferDocuments(DateTime? cd = null)
        {
            ViewBag.toolChosen = "EditOfferDoc";
            ViewBag.dateCreated = cd;
            List<UserDocsClient> debDocumentsInfoList = TempDataManager.LoadTempData(this, "debOfferDocumentsInfo") as List<UserDocsClient>;
            var objUserDocument = debDocumentsInfoList.Where(d => d.Created == cd).FirstOrDefault();
            if (objUserDocument != null)
            {
                return View("DebOfferDocuments_Model", objUserDocument);
            }
            return RedirectToAction("Login", "Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditDebOfferDocuments(UserDocsClient objDocumentsInfo, FormCollection form)
        {
            DateTime cd = DateTime.Parse(form["hdnCD1"]);

            if (ModelState.IsValid)
            {
                List<UserDocsClient> debDocumentsInfoList = TempDataManager.LoadTempData(this, "debOfferDocumentsInfo") as List<UserDocsClient>;
                var oldUserDocument = debDocumentsInfoList.Where(d => d.Created == cd).FirstOrDefault();
                if (oldUserDocument == null || debDocumentsInfoList == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                var clone = StreamingManager.Clone(oldUserDocument) as UserDocsClient;
                var values = JsonConvert.SerializeObject(form.AllKeys.ToDictionary(k => k, k => form[k]));
                JsonConvert.PopulateObject(values, clone);
                clone.Created = oldUserDocument.Created;
                clone._Created = oldUserDocument._Created;
                var err = await DataQuery.Update(oldUserDocument, clone);
                if (err == ErrorCodes.Succes)
                {
                    int indrow = debDocumentsInfoList.IndexOf(oldUserDocument);
                    debDocumentsInfoList.RemoveAt(indrow);
                    debDocumentsInfoList.Insert(indrow, clone);
                    TempDataManager.SaveTempData(this, "debOfferDocumentsInfo", debDocumentsInfoList);

                    return Json(new { success = true, message = string.Format(Localization.lookup("SavedOBJ"), string.Empty) });
                }
                else
                    return await getJsonError(err);
            }
            else
            {
                return getModelStateError();
            }
        }

        public ActionResult DeleteDebOfferDocuments(DateTime? cd = null)
        {

            ViewBag.toolChosen = "DeleteOfferDoc";
            ViewBag.dateCreated = cd;
            List<UserDocsClient> debDocumentsInfoList = TempDataManager.LoadTempData(this, "debOfferDocumentsInfo") as List<UserDocsClient>;
            var dbUserDocument = debDocumentsInfoList.Where(d => d.Created == cd).FirstOrDefault();
            if (dbUserDocument != null)
            {
                return View("DebOfferDocuments_Model", dbUserDocument);
            }
            return RedirectToAction("Login", "Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteDebOfferDocuments(FormCollection form)
        {
            DateTime cd = DateTime.Parse(form["hdnCD1"]);

            List<UserDocsClient> debDocumentsInfoList = TempDataManager.LoadTempData(this, "debOfferDocumentsInfo") as List<UserDocsClient>;
            var dbUserDocument = debDocumentsInfoList.Where(d => d.Created == cd).FirstOrDefault();

            if (dbUserDocument != null)
            {
                var err = await DataQuery.Delete(dbUserDocument);
                if (err == ErrorCodes.Succes)
                {
                    debDocumentsInfoList.Remove(dbUserDocument);
                    TempDataManager.SaveTempData(this, "debOfferDocumentsInfo", debDocumentsInfoList);
                    return Json(new { success = true, message = Localization.lookup("Deleted"), string.Empty });
                }
                else
                {
                    return await getJsonError(err);
                }
            }
            else
            {
                return RedirectToAction("Login", "Login");
            }
        }


        #endregion

        #region Convert QuotToOrd

        public async Task<ActionResult> ConvertQuotToOrder(int rowId = 0)
        {
            SQLCache debtorOffers = si.CurrentCompany.GetCache(typeof(DebtorOffer));
            if (debtorOffers == null)
                debtorOffers = await si.CurrentCompany.LoadCache(typeof(DebtorOffer), si.BaseAPI).ConfigureAwait(true);

            List<DebtorOfferPortal> DebtorOfferList = TempDataManager.LoadTempData(this, "debOffers") as List<DebtorOfferPortal>;
            DebtorOfferPortal curDebtorOffer = new DebtorOfferPortal();

            if (DebtorOfferList != null)
            {
                curDebtorOffer = DebtorOfferList.Where(d => d.RowId == rowId).FirstOrDefault();
            }

            if (ModelState.IsValid)
            {
                var odrApi = new OrderAPI(si.BaseAPI);
                ErrorCodes res = await odrApi.ConvertOfferToOrder(curDebtorOffer);
                if (res == ErrorCodes.Succes)
                {
                    return Json(new { success = true, message = string.Format(Localization.lookup("SavedOBJ"), string.Empty) }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return await getJsonError(res);
                }
            }
            else
            {
                return getModelStateError();
            }
        }
        #endregion

        #region Create Sales/Purchage Order

        [HttpGet]
        public ActionResult CreateSalePurchaseOrder(int rowId = 0)
        {
            ViewBag.RowId = rowId;
            var orderTypes = new string[] { Uniconta.ClientTools.Localization.lookup("SalesOrder"), Uniconta.ClientTools.Localization.lookup("PurchaseOrder"), Uniconta.ClientTools.Localization.lookup("Offer") };
            ViewData["DCOrdType"] = orderTypes;
            return View("CreateOrderByQuotation");
        }
        [HttpPost]
        public async Task<ActionResult> CreateSalePurchaseOrder(FormCollection form)
        {
            int rowId = Convert.ToInt32(form["RowId"]);
            int idx = Convert.ToInt32(form["SelcIndex"]);
            var account = Convert.ToString(form["Account"]);
            bool isDebtorOrder = form["isDebtor"] == "true" ? true : false;
            bool inverSign = form["InverSign"] == "true" ? true : false;
            bool copyAttachment = form["copyAttachment"] == "true" ? true : false;
            bool copyDelAddress = form["copyDeliveryAddress"] == "true" ? true : false;
            bool reCalPrice = form["reCalculatePrice"] == "true" ? true : false;
            bool orderPerAcc = form["orderPerPurchaseAccount"] == "true" ? true : false;
            int ordNum = -1;
            string confirmationMsg = string.Empty;
            string title = Localization.lookup("Confirmation");
            if (!orderPerAcc && string.IsNullOrEmpty(account))
                return Json(new { success = false, message = string.Format(Localization.lookup("FieldCannotBeEmpty"), string.Empty) });

            var orderApi = new OrderAPI(si.BaseAPI);
            var dcOrder = getDCOrder(idx);
            List<DebtorOfferPortal> DebtorOfferList = TempDataManager.LoadTempData(this, "debOffers") as List<DebtorOfferPortal>;
            DebtorOfferPortal debOffer = DebtorOfferList.Where(d => d.RowId == rowId).FirstOrDefault();

            if (ModelState.IsValid)
            {
                var err = await orderApi.CreateOrderFromOrder(debOffer, dcOrder, account, inverSign, CopyAttachments: copyAttachment, CopyDeliveryAddress: copyDelAddress, RecalculatePrices: reCalPrice, OrderPerPurchaseAccount: orderPerAcc);
                if (err == ErrorCodes.Succes)
                {
                    if (dcOrder != null)
                        ordNum = dcOrder._OrderNumber;

                    if (idx == 0)
                        confirmationMsg = string.Format("{0} {1} {2}", Localization.lookup("SalesOrderCreated") + ".", Localization.lookup("OrderNumber") + ": " + ordNum, Localization.lookup("Account") + ": " + account + ", ") + string.Format(Localization.lookup("GoTo"), Localization.lookup("OrdersLine") + "?");
                    else if (idx == 1)
                        confirmationMsg = string.Format("{0} {1} {2}", Localization.lookup("PurchaseOrderCreated") + ".", Localization.lookup("OrderNumber") + ": " + ordNum, Localization.lookup("Account") + ": " + account + ", ") + string.Format(Localization.lookup("GoTo"), Localization.lookup("PurchaseLines") + "?");
                    else if (idx == 2)
                        confirmationMsg = string.Format("{0} {1} {2}", Localization.lookup("OfferOrderCreated") + ".", Localization.lookup("OrderNumber") + ": " + ordNum, Localization.lookup("Account") + ": " + account + ", ") + string.Format(Localization.lookup("GoTo"), Localization.lookup("OfferLine") + "?");

                    return Json(new { success = "CreateSPOrd", rowId = rowId, sIndex = idx, ordNum = ordNum, account = account, title = title, cMsg = confirmationMsg, message = string.Format(Localization.lookup("SavedOBJ"), string.Empty) });
                }
                else
                {
                    return await getJsonError(err);
                }
            }
            else
                return getModelStateError();
        }

        public ActionResult checkValidity(string value, string account)
        {
            int selctedIndex = Convert.ToInt32(value);
            string msg = string.Empty;
            string title = Localization.lookup("Warning");
            DCAccount dcAccount;
            if (selctedIndex == 0)
            {
                //orderPerAcc = false;
                dcAccount = ClientHelper.GetRef(si.BaseAPI.CompanyId, typeof(Debtor), account) as DCAccount;
            }
            else
            {
                dcAccount = ClientHelper.GetRef(si.BaseAPI.CompanyId, typeof(Creditor), account) as DCAccount;
            }

            if (dcAccount != null)
            {
                msg = string.Format("{0}. {1}", Uniconta.ClientTools.Localization.lookup("AccountIsBlocked"), Uniconta.ClientTools.Localization.lookup("ProceedConfirmation"));
                if (!UtilDisplay.IsExecuteWithBlockedAccount(dcAccount))
                    //return Json(new { success = "AccountBlocked" }, JsonRequestBehavior.AllowGet);
                    return Json(new { success = "AccountBlocked", message = msg, title = title }, JsonRequestBehavior.AllowGet);

            }
            return Json(new { success = "ValidAccount" }, JsonRequestBehavior.AllowGet);
        }

        public DCOrder getDCOrder(int idx)
        {
            Type t;
            if (idx == 2)
                t = typeof(DebtorOfferClient);
            else if (idx == 1)
                t = typeof(CreditorOrderClient);
            else
                t = typeof(DebtorOrderClient);
            return (DCOrder)Activator.CreateInstance(Global.GetTableWithUserFields(si.BaseAPI.CompanyEntity, t, true));
        }

        public async Task<ActionResult> getAccountsByDCType(string value, DataSourceLoadOptions loadOptions)
        {
            int selctedIndex = Convert.ToInt32(value);
            var api = si.BaseAPI;
            SQLCache DebtorCache, CreditorCache, cache;
            List<IdKeyClient> pairs = new List<IdKeyClient>();
            var Comp = api.CompanyEntity;

            if (selctedIndex == 1)
                cache = CreditorCache = Comp.GetCache(typeof(Uniconta.DataModel.Creditor)) ?? await Comp.LoadCache(typeof(Uniconta.DataModel.Creditor), api).ConfigureAwait(false);
            else
                cache = DebtorCache = Comp.GetCache(typeof(Uniconta.DataModel.Debtor)) ?? await Comp.LoadCache(typeof(Uniconta.DataModel.Debtor), api).ConfigureAwait(false);

            var IdKeyNames = (cache?.GetNotNullArray) as IList<IdKeyName>;
            if (IdKeyNames != null)
                pairs = (from s in IdKeyNames
                         select new IdKeyClient
                         {
                             KeyName = s.KeyName,
                             KeyStr = s.KeyStr
                         }).ToList();
            else
            {
                var IdKeys = ((IList<IdKey>)cache);
                pairs = (from s in IdKeys select new IdKeyClient { KeyStr = s.KeyStr, KeyName = string.Empty }).ToList();
            }
            return Content(JsonConvert.SerializeObject(DataSourceLoader.Load(pairs, loadOptions)), "application/json");
        }

        #endregion

        #region Inventory OnHand
        public async Task<ActionResult> Storage(int rowId = 0)
        {
            SQLCache debtorOfferLines = si.CurrentCompany.GetCache(typeof(DebtorOffer));
            if (debtorOfferLines == null)
                debtorOfferLines = await si.CurrentCompany.LoadCache(typeof(DebtorOffer), si.BaseAPI).ConfigureAwait(true);

            ViewBag.RowId = rowId;
            ViewBag.IsNotForAllInvStorage = false;
            ViewBag.CompanyDetatil = si.CurrentCompany;

            TempDataManager.RemoveTempData(this, "offerInvStorage");
            if (rowId != 0)
            {
                ViewBag.IsNotForAllInvStorage = true;
                List<DebtorOfferLinePortal> DebtorOfferLineList = TempDataManager.LoadTempData(this, "dbOfferLines") as List<DebtorOfferLinePortal>;
                if (DebtorOfferLineList == null)
                {
                    var res = await FilterGrid(typeof(InvItemClient), null, null);
                    DebtorOfferLineList = GetList(res);
                }
                var acDetailsById = DebtorOfferLineList.Where(s => s.RowId == rowId).FirstOrDefault();
                if (acDetailsById != null)
                {
                    ViewBag.Item = acDetailsById.Account;
                    ViewBag.ItemName = acDetailsById.Name;
                }
                return View("_PartialStorage");
            }
            return HttpNotFound();
            //else
            //{
            //    TempData.Clear();
            //    return View("InventoryStorage");
            //}
        }

        public async Task<ActionResult> LoadStorageDetail(DataSourceLoadOptions loadOptions, string IsGridRefreshed, int rowId = 0)
        {
            bool IsRefreshed = false;
            if (!String.IsNullOrEmpty(IsGridRefreshed))
            {
                IsRefreshed = Convert.ToBoolean(IsGridRefreshed);
            }

            List<InvItemStorageClientLocal> InvStorageList = TempDataManager.LoadTempData(this, "offerInvStorage") as List<InvItemStorageClientLocal>;

            List<DebtorOfferLinePortal> DebtorOfferLineList = TempDataManager.LoadTempData(this, "dbOfferLines") as List<DebtorOfferLinePortal>;
            var storageDetailsById = DebtorOfferLineList.Where(s => s.RowId == rowId).FirstOrDefault();
            if (IsRefreshed || (storageDetailsById != null && InvStorageList == null) || (storageDetailsById != null && InvStorageList.Count == 0))
            {
                var res = await FilterGrid(typeof(InvItemStorageClientLocal), new List<UnicontaBaseEntity>() { storageDetailsById });
                if (res != null)
                {
                    InvStorageList = res.OfType<InvItemStorageClientLocal>().ToList();
                    TempDataManager.SaveTempData(this, "offerInvStorage", InvStorageList);
                }
                return Content(JsonConvert.SerializeObject(DataSourceLoader.Load(InvStorageList, loadOptions), new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), "application/json");
            }
            else if (storageDetailsById != null && InvStorageList != null)
            {
                return Content(JsonConvert.SerializeObject(DataSourceLoader.Load(InvStorageList, loadOptions), new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), "application/json");
            }
            return HttpNotFound();

        }
        #endregion

        #region Update Document
        public ActionResult GenerateInvoiceOrderLine()
        {
            DebtorOrderPortal OpenDeptorOrder = TempDataManager.LoadTempData(this, "dbOpenOrder") as DebtorOrderPortal;
            if (OpenDeptorOrder != null)
            {
                return RedirectToAction("ValidateVatAndCurrency", new { rowId = OpenDeptorOrder.RowId });
            }
            else
            {
                return RedirectToAction("Logout", "DashBoards");
            }

        }


        public ActionResult ValidateVatAndCurrency(int rowId = 0)
        {
            InvoiceAPI Invapi = new InvoiceAPI(si.BaseAPI);
            bool IsValid = true;
            string message = string.Empty;
            string title = Localization.lookup("Warning");
            List<DebtorOrderPortal> DebtorOrderslist = TempDataManager.LoadTempData(this, "dbOrders") as List<DebtorOrderPortal>;
            DebtorOrderPortal dbOrder = DebtorOrderslist.Where(d => d.RowId == rowId).FirstOrDefault();
            var debtor = ClientHelper.GetRef(dbOrder.CompanyId, typeof(Debtor), dbOrder._DCAccount) as Debtor;
            if (debtor != null)
            {
                var InvoiceAccount = dbOrder._InvoiceAccount ?? debtor._InvoiceAccount;
                if (InvoiceAccount != null)
                    debtor = ClientHelper.GetRef(dbOrder.CompanyId, typeof(Debtor), InvoiceAccount) as Debtor;
                if (debtor != null)
                {
                    if (debtor._PricesInclVat != dbOrder._PricesInclVat)
                    {
                        IsValid = false;
                        message = string.Format("{0}.\n{1}",
                            string.Format(Localization.lookup("DebtorAndOrderMix"), Localization.lookup("InclVat")),
                        Localization.lookup("ProceedConfirmation"));

                    }
                    if (!si.BaseAPI.CompanyEntity.SameCurrency(dbOrder._Currency, debtor._Currency))
                    {
                        IsValid = false;
                        message = string.Format("{0}.\n{1}", string.Format(Localization.lookup("CurrencyMismatch"), AppEnums.Currencies.ToString((int)debtor._Currency), dbOrder.Currency),
                       Localization.lookup("ProceedConfirmation"));

                    }
                }
            }
            return Json(new { IsValid = IsValid, message = message, title = title, RowId = rowId }, JsonRequestBehavior.AllowGet);

        }
        [HttpGet]
        public ActionResult GenerateInvoice(string DocType, int rowId = 0)
        {
            List<DebtorOrderPortal> DebtorOrderslist = TempDataManager.LoadTempData(this, "dbOrders") as List<DebtorOrderPortal>;
            DebtorOrderPortal debtorOrder = DebtorOrderslist.Where(d => d.RowId == rowId).FirstOrDefault();
            ViewBag.TitleDoc = Localization.lookup(Localization.lookup("GenerateInvoice"));
            ViewBag.isShowInvoiceVisible = true;
            ViewBag.IsSimulation = true;
            ViewBag.PrintPreview = true;
            ViewBag.showInputforInvNumber = false;
            ViewBag.askForEmail = true;
            ViewBag.showInvoice = true;
            InvoiceAPI Invapi = new InvoiceAPI(si.BaseAPI);
            bool showSendByMail = false;
            var debtor = ClientHelper.GetRef(debtorOrder.CompanyId, typeof(Debtor), debtorOrder._DCAccount) as Debtor;
            if (debtor != null)
            {
                var InvoiceAccount = debtorOrder._InvoiceAccount ?? debtor._InvoiceAccount;
                if (InvoiceAccount != null)
                    debtor = ClientHelper.GetRef(debtorOrder.CompanyId, typeof(Debtor), InvoiceAccount) as Debtor;
            }
            showSendByMail = !string.IsNullOrEmpty(debtor._InvoiceEmail);
            string debtorName = debtor?._Name ?? debtorOrder._DCAccount;
            ViewBag.debtorName = debtorName;
            ViewBag.isShowUpdateInv = false;
            ViewBag.showNoEmailMsg = !showSendByMail;
            ViewBag.txtNoMailMsg = string.Format(Localization.lookup("DebtorHasNoEmail"), debtorName);
            ViewBag.FormAction = "/DebtorAccount/DebtorUpdateDocument/";
            ViewBag.compLayoutType = DocType;
            ViewBag.GenrateDate = DateTime.Now;
            return View("UpdateDocument");
        }
        [HttpGet]
        public ActionResult DebtorUpdateDocument(string DocType, int rowId = 0)
        {
            List<DebtorOrderPortal> DebtorOrderslist = TempDataManager.LoadTempData(this, "dbOrders") as List<DebtorOrderPortal>;
            DebtorOrderPortal debtorOrder = DebtorOrderslist.Where(d => d.RowId == rowId).FirstOrDefault();
            CompanyLayoutType compLayoutType = CompanyLayoutType.Invoice;


            switch (DocType)
            {
                case "OrderConfirmation":
                    compLayoutType = CompanyLayoutType.OrderConfirmation;
                    ViewBag.isShowInvoiceVisible = true;
                    ViewBag.IsSimulation = false;
                    ViewBag.PrintPreview = true;
                    ViewBag.showInputforInvNumber = false;
                    ViewBag.askForEmail = true;
                    ViewBag.showInvoice = true;
                    ViewBag.TitleDoc = Localization.lookup(compLayoutType.ToString());
                    break;
                case "PackNote":
                    compLayoutType = CompanyLayoutType.Packnote;
                    ViewBag.isShowInvoiceVisible = true;
                    ViewBag.IsSimulation = false;
                    ViewBag.PrintPreview = true;
                    ViewBag.showInputforInvNumber = false;
                    ViewBag.askForEmail = true;
                    ViewBag.showInvoice = true;
                    ViewBag.TitleDoc = Localization.lookup(compLayoutType.ToString());
                    break;
                case "PickList":
                    //compLayoutType = CompanyLayoutType.PickingList;
                    //ViewBag.isShowInvoiceVisible = true;
                    //ViewBag.IsSimulation = false;
                    //ViewBag.PrintPreview = true;
                    //ViewBag.showInputforInvNumber = false;
                    //ViewBag.askForEmail = true;
                    //ViewBag.showInvoice = true;
                    ViewBag.TitleDoc = string.Format("{0} {1}", Localization.lookup("Select"), Localization.lookup("Date"));
                    goto DateBoxFormControl;

                case "Invoice":

                    return RedirectToAction("GenerateInvoice", new { DocType = "Invoice", rowId = rowId });


            }


            InvoiceAPI Invapi = new InvoiceAPI(si.BaseAPI);
            var debtor = debtorOrder.Debtor;
            bool showSendByMail = true;
            if (debtor != null)
                showSendByMail = !string.IsNullOrEmpty(debtor.InvoiceEmail);
            string debtorName = debtor?._Name ?? debtorOrder._DCAccount;
            ViewBag.isShowUpdateInv = si.BaseAPI.CompanyEntity.Storage;
            ViewBag.showNoEmailMsg = !showSendByMail;

            ViewBag.debtorName = debtorName;
            ViewBag.txtNoMailMsg = string.Format(Localization.lookup("DebtorHasNoEmail"), debtorName);
            DateBoxFormControl: ViewBag.FormAction = "/DebtorAccount/DebtorUpdateDocument/";
            ViewBag.GenrateDate = DateTime.Now;
            ViewBag.compLayoutType = DocType;
            return View("UpdateDocument");
        }
        [HttpPost]
        public async Task<ActionResult> DebtorUpdateDocument(FormCollection form)
        {

            string DocType = Convert.ToString(form["hdnDocType"]);
            CompanyLayoutType compLayoutType = GetCompanyLayoutType(DocType);


            if (compLayoutType == CompanyLayoutType.OrderConfirmation || compLayoutType == CompanyLayoutType.Packnote)
            {
                return await OrderConfirmation(form);
            }
            else if (compLayoutType == CompanyLayoutType.PickingList)
            {
                return await PickingListConfirm(form);
            }
            if (compLayoutType == CompanyLayoutType.PurchaseInvoice)
            {
                return await GenerateInvoice(form);
            }
            else
            {

                return await OrderConfirmation(form);
            }


        }

        public async Task<ActionResult> GenerateInvoice(FormCollection form)
        {
            int rowId = Convert.ToInt32(form["RowId"]);
            PrintPreview objPrintPreview = new PrintPreview();
            objPrintPreview.rowId = rowId;
            objPrintPreview.ShowInvoice = Convert.ToBoolean(form["ShowPrint"]);
            objPrintPreview.GenrateDate = Convert.ToDateTime(form["Date"]);
            objPrintPreview.UpdateInventory = Convert.ToBoolean(form["UpdateInventory"]);
            objPrintPreview.SendByEmail = Convert.ToBoolean(form["SendInvoiceByEmail"]);
            objPrintPreview.DocType = Convert.ToString(form["hdnDocType"]);
            objPrintPreview.Emails = Convert.ToString(form["Email"]);
            objPrintPreview.sendOnlyToThisEmail = Convert.ToBoolean(form["SendOnlyToThisEmail"]);
            objPrintPreview.IsSimulation = Convert.ToBoolean(form["Simulation"]);
            InvoiceAPI Invapi = new InvoiceAPI(si.BaseAPI);
            List<DebtorOrderPortal> DebtorOrderslist = TempDataManager.LoadTempData(this, "dbOrders") as List<DebtorOrderPortal>;
            DebtorOrderPortal debtorOrder = DebtorOrderslist.Where(d => d.RowId == rowId).FirstOrDefault();

            var result = await Invapi.PostInvoice(debtorOrder, null, objPrintPreview.GenrateDate, 0,
                        objPrintPreview.IsSimulation, new DebtorInvoiceClient(),
                        new DebtorInvoiceLines(),
                        objPrintPreview.SendByEmail, objPrintPreview.ShowInvoice, Emails: objPrintPreview.Emails, OnlyToThisEmail: objPrintPreview.sendOnlyToThisEmail, GLTransType: new GLTransClientTotal());

            if (result.ledgerRes.Err == ErrorCodes.Succes)
            {
                //if (!objPrintPreview.IsSimulation && debtorOrder._DeleteLines)
                //reloadTask = BindGrid();


                string msg = string.Empty;
                if (result.Header._InvoiceNumber != 0)
                {
                    msg = string.Format(Localization.lookup("InvoiceHasBeenGenerated"), result.Header._InvoiceNumber);
                    msg = string.Format("{0}{1}{2} {3}", msg, Environment.NewLine, Localization.lookup("LedgerVoucher"), result.Header._Voucher);
                }
                else if (objPrintPreview.ShowInvoice)
                {
                    msg = Localization.lookup("InvoiceProposal");
                }
                // lest save our panel before opening a new, so we close the correct one.
                if (objPrintPreview.ShowInvoice)
                {
                    return Json(new { success = "UpdateDocTrue", ShowSimulatedTransactions = false, ShowInvoice = true, PrintPreviewParams = objPrintPreview.GetQueryString(), message = msg });
                }
                else if (!objPrintPreview.ShowInvoice && objPrintPreview.IsSimulation && result.ledgerRes?.SimulatedTrans != null && result.ledgerRes.SimulatedTrans.Length > 0)
                {
                    //object[] paramSimulatedTrans = new object[2];
                    //paramSimulatedTrans[0] = result.ledgerRes.AccountBalance;
                    //paramSimulatedTrans[1] = result.ledgerRes.SimulatedTrans;
                    //AddDockItem(TabControls.SimulatedTransactions, paramSimulatedTrans, Localization.lookup("SimulatedTransactions"), null, true);
                    return Json(new { success = "UpdateDocTrue", ShowSimulatedTransactions = true, ShowInvoice = false, PrintPreviewParams = objPrintPreview.GetQueryString(), message = msg });
                }
                else
                {
                    return Json(new { success = "UpdateDocTrue", ShowSimulatedTransactions = false, ShowInvoice = false, PrintPreviewParams = string.Empty, message = msg });
                }
            }
            else
            {
                return await getJsonError(result.ledgerRes.Err);
            }
        }
        public async Task<ActionResult> PickingListConfirm(FormCollection form)
        {
            int rowId = Convert.ToInt32(form["RowId"]);
            DateTime selectedDate = Convert.ToDateTime(form["selectedDate"]);
            PrintPreview objPrintPreview = new PrintPreview();
            objPrintPreview.SelectedDate = selectedDate;
            objPrintPreview.DocType = Convert.ToString(form["hdnDocType"]);
            objPrintPreview.rowId = rowId;
            InvoiceAPI Invapi = new InvoiceAPI(si.BaseAPI);
            List<DebtorOrderPortal> DebtorOrderslist = TempDataManager.LoadTempData(this, "dbOrders") as List<DebtorOrderPortal>;
            DebtorOrderPortal debtorOrder = DebtorOrderslist.Where(d => d.RowId == rowId).FirstOrDefault();
            var debtor = debtorOrder.Debtor;
            string debtorName = debtor?._Name ?? debtorOrder._DCAccount;
            var result = await Invapi.PostInvoice(debtorOrder, null, selectedDate, 0, false, new DebtorInvoiceClient(), new DebtorInvoiceLines(), false, true, CompanyLayoutType.PickingList);
            if (result.Err == ErrorCodes.Succes)
            {
                return Json(new { success = "UpdateDocTrue", ShowInvoice = true, PrintPreviewParams = objPrintPreview.GetQueryString(), message = string.Format(Localization.lookup("SavedOBJ"), string.Empty) });

            }
            else
            {
                return await getJsonError(result.Err);
            }
        }

        public async Task<ActionResult> OrderConfirmation(FormCollection form)
        {
            int rowId = Convert.ToInt32(form["RowId"]);
            PrintPreview objPrintPreview = new PrintPreview();
            objPrintPreview.rowId = rowId;
            objPrintPreview.ShowInvoice = Convert.ToBoolean(form["ShowPrint"]);
            objPrintPreview.GenrateDate = Convert.ToDateTime(form["Date"]);
            objPrintPreview.UpdateInventory = Convert.ToBoolean(form["UpdateInventory"]);
            objPrintPreview.SendByEmail = Convert.ToBoolean(form["SendInvoiceByEmail"]);
            objPrintPreview.DocType = Convert.ToString(form["hdnDocType"]);
            objPrintPreview.Emails = Convert.ToString(form["Email"]);
            objPrintPreview.sendOnlyToThisEmail = Convert.ToBoolean(form["SendOnlyToThisEmail"]);

            InvoiceAPI Invapi = new InvoiceAPI(si.BaseAPI);
            List<DebtorOrderPortal> DebtorOrderslist = TempDataManager.LoadTempData(this, "dbOrders") as List<DebtorOrderPortal>;
            DebtorOrderPortal debtorOrder = DebtorOrderslist.Where(d => d.RowId == rowId).FirstOrDefault();
            var result = await Invapi.PostInvoice(debtorOrder, null, objPrintPreview.GenrateDate, 0,
                       !objPrintPreview.UpdateInventory, new DebtorInvoiceClient(),
                       new DebtorInvoiceLines(),
                   objPrintPreview.SendByEmail, objPrintPreview.ShowInvoice, objPrintPreview.isPacknote ? CompanyLayoutType.Packnote : CompanyLayoutType.OrderConfirmation, objPrintPreview.Emails, objPrintPreview.sendOnlyToThisEmail);


            if (result.Err == ErrorCodes.Succes)
            {
                if (objPrintPreview.UpdateInventory)
                {
                    var loaded = StreamingManager.Clone(debtorOrder) as DebtorOrderPortal;

                    if (objPrintPreview.isPacknote)
                        debtorOrder._PackNotePrinted = DateTime.Now;
                    else
                        debtorOrder._ConfirmPrinted = DateTime.Now;

                    ErrorCodes res1 = await si.BaseAPI.Update(loaded, debtorOrder);
                    if (res1 == ErrorCodes.Succes)
                    {
                        int indrow = DebtorOrderslist.IndexOf(loaded);
                        DebtorOrderslist.RemoveAt(indrow);
                        DebtorOrderslist.Insert(indrow, debtorOrder);
                        TempDataManager.SaveTempData(this, "dbOrders", DebtorOrderslist);
                    }
                }
                if (objPrintPreview.ShowInvoice)
                {

                    return Json(new { success = "UpdateDocTrue", ShowInvoice = true, PrintPreviewParams = objPrintPreview.GetQueryString(), message = string.Format(Localization.lookup("SavedOBJ"), string.Empty) });
                }
                else
                {

                    return Json(new { success = "UpdateDocTrue", ShowInvoice = false, PrintPreviewParams = string.Empty, message = string.Format(Localization.lookup("SavedOBJ"), string.Empty) });
                }
            }
            else
            {
                return await getJsonError(result.Err);
            }
        }

        async Task<IPrintReport> OfferToPrint(InvoicePostingResult res, DebtorOrderClient dbOrder, CompanyLayoutType docName)

        {
            IPrintReport iprintReport = null;
            try
            {

                var docVersion = docName == CompanyLayoutType.Packnote ? StandardReports.PackNote : docName == CompanyLayoutType.OrderConfirmation ?
                    StandardReports.OrderConfirmation : StandardReports.SalesPickingList;
                CrudAPI api = new CrudAPI(si.BaseAPI);
                var debtorInvoicePrint = new DebtorInvoicePrintReport(res, api, docName, dbOrder);
                if (debtorInvoicePrint != null)
                {
                    await debtorInvoicePrint.InstantiateFields();
                    var standardReports = new IDebtorStandardReport[1];

                    if (docName != CompanyLayoutType.PickingList)
                    {
                        var standardDebtorReport = new DebtorQCPReportClient(debtorInvoicePrint.Company, debtorInvoicePrint.Debtor, debtorInvoicePrint.DebtorInvoice, debtorInvoicePrint.InvTransInvoiceLines, debtorInvoicePrint.DebtorOrder,
                            debtorInvoicePrint.DocumentClient.DocumentData, debtorInvoicePrint.ReportName, (byte)docVersion, messageClient: debtorInvoicePrint.MessageClient);
                        standardReports[0] = standardDebtorReport;
                    }
                    else
                    {
                        var standardDebtorPickingList = new DebtorSalesPickingListReportClient(debtorInvoicePrint.Company, debtorInvoicePrint.Debtor, debtorInvoicePrint.DebtorInvoice, debtorInvoicePrint.InvTransInvoiceLines, debtorInvoicePrint.DebtorOrder,
                            debtorInvoicePrint.DocumentClient.DocumentData, debtorInvoicePrint.ReportName);
                        standardReports[0] = standardDebtorPickingList;
                    }

                    iprintReport = new StandardPrintReport(api, standardReports, (byte)docVersion);
                    await iprintReport.InitializePrint();
                }

                //If standard Report doesn't exist look for Layout
                if (iprintReport?.Report == null)
                {
                    iprintReport = new LayoutPrintReport(api, res, docName);
                    await iprintReport.InitializePrint();
                }

                if (iprintReport == null || iprintReport.Report == null)
                {

                }

                var iReports = new IPrintReport[1] { iprintReport };
                var reportName = Localization.lookup(docName.ToString());


            }
            catch (Exception ex)
            {
                throw ex;
            }
            return iprintReport;
        }
        private CompanyLayoutType GetCompanyLayoutType(string DocType)
        {
            CompanyLayoutType compLayoutType = CompanyLayoutType.Invoice;
            switch (DocType)
            {
                case "OrderConfirmation":
                    compLayoutType = CompanyLayoutType.OrderConfirmation;

                    break;
                case "PackNote":
                    compLayoutType = CompanyLayoutType.Packnote;

                    break;
                case "PickList":
                    compLayoutType = CompanyLayoutType.PickingList;

                    break;
                case "Invoice":
                    compLayoutType = CompanyLayoutType.PurchaseInvoice;

                    break;
            }
            return compLayoutType;
        }
        public async Task<ActionResult> ShowInvoice()
        {
            PrintPreview objPrintPreview = QueryStringHelper.GetFromQueryString<PrintPreview>();
            InvoicePostingResult InvoicePostingResult = new InvoicePostingResult();
            InvoiceAPI Invapi = new InvoiceAPI(si.BaseAPI);
            List<DebtorOrderPortal> DebtorOrderslist = TempDataManager.LoadTempData(this, "dbOrders") as List<DebtorOrderPortal>;
            DebtorOrderPortal debtorOrder = DebtorOrderslist.Where(d => d.RowId == objPrintPreview.rowId).FirstOrDefault();
            CompanyLayoutType compLayoutType = GetCompanyLayoutType(objPrintPreview.DocType);
            IPrintReport printReport = null;

            if (compLayoutType == CompanyLayoutType.OrderConfirmation || compLayoutType == CompanyLayoutType.Packnote)
            {
                InvoicePostingResult = await Invapi.PostInvoice(debtorOrder, null, objPrintPreview.GenrateDate, 0,
                        !objPrintPreview.UpdateInventory, new DebtorInvoiceClient(),
                        new DebtorInvoiceLines(),
                    objPrintPreview.SendByEmail, objPrintPreview.ShowInvoice, objPrintPreview.isPacknote ? CompanyLayoutType.Packnote : CompanyLayoutType.OrderConfirmation, objPrintPreview.Emails, objPrintPreview.sendOnlyToThisEmail);
                printReport = await OfferToPrint(InvoicePostingResult, debtorOrder, compLayoutType);
            }
            else if (compLayoutType == CompanyLayoutType.PickingList)
            {
                InvoicePostingResult = await Invapi.PostInvoice(debtorOrder, null, objPrintPreview.SelectedDate, 0, false, new DebtorInvoiceClient(), new DebtorInvoiceLines(), false, true, CompanyLayoutType.PickingList);
                printReport = await OfferToPrint(InvoicePostingResult, debtorOrder, compLayoutType);
            }
            else if (compLayoutType == CompanyLayoutType.PurchaseInvoice)
            {
                InvoicePostingResult = await Invapi.PostInvoice(debtorOrder, null, objPrintPreview.GenrateDate, 0,
                        objPrintPreview.IsSimulation, new DebtorInvoiceClient(),
                        new DebtorInvoiceLines(),
                        objPrintPreview.SendByEmail, objPrintPreview.ShowInvoice, Emails: objPrintPreview.Emails, OnlyToThisEmail: objPrintPreview.sendOnlyToThisEmail, GLTransType: new GLTransClientTotal());
                printReport = await GenerateInvoiceToPrint(InvoicePostingResult, debtorOrder);
            }


            var model = new ReportsModel
            {
                ReportName = string.Format("{0}: {1}", Localization.lookup("PrintPreview"), Localization.lookup(compLayoutType.ToString())),
                Report = printReport.Report
            };
            return View("InvoiceViewer", model);

        }

        public ActionResult SimulatedTransactions()
        {
            ViewBag.CurrentCompany = si.CurrentCompany;
            ViewBag.SimulatedTransParam = Request.QueryString.ToString();
            ViewBag.DataSourceController = "DebtorAccount";
            return View();

        }
        public async Task<ActionResult> LoadSimulatedTransactions(DataSourceLoadOptions loadOptions, string SimulatedTransParam)
        {
            PrintPreview objPrintPreview = QueryStringHelper.GetFromQueryString<PrintPreview>(SimulatedTransParam);
            InvoiceAPI Invapi = new InvoiceAPI(si.BaseAPI);
            List<DebtorOrderPortal> DebtorOrderslist = TempDataManager.LoadTempData(this, "dbOrders") as List<DebtorOrderPortal>;
            DebtorOrderPortal debtorOrder = DebtorOrderslist.Where(d => d.RowId == objPrintPreview.rowId).FirstOrDefault();

            var result = await Invapi.PostInvoice(debtorOrder, null, objPrintPreview.GenrateDate, 0,
                        objPrintPreview.IsSimulation, new DebtorInvoiceClient(),
                        new DebtorInvoiceLines(),
                        objPrintPreview.SendByEmail, objPrintPreview.ShowInvoice, Emails: objPrintPreview.Emails, OnlyToThisEmail: objPrintPreview.sendOnlyToThisEmail, GLTransType: new GLTransClientTotal());
            UnicontaBaseEntity[] simulatedTransactions = null;
            if (result.ledgerRes.Err == ErrorCodes.Succes)
            {

                simulatedTransactions = result.ledgerRes.SimulatedTrans;
            }
            GLTransClientTotal[] lst = null;
            if (simulatedTransactions != null && simulatedTransactions.Length > 0)
            {
                lst = new GLTransClientTotal[simulatedTransactions.Length];
                long total = 0;
                int i = 0;
                foreach (var t in (IEnumerable<GLTransClientTotal>)simulatedTransactions)
                {
                    total += t._AmountCent;
                    t._Total = total;
                    lst[i++] = t;
                }

            }
            return Content(JsonConvert.SerializeObject(DataSourceLoader.Load(lst, loadOptions), JsonSettings), "application/json");

        }




        async Task<IPrintReport> GenerateInvoiceToPrint(InvoicePostingResult res, DebtorOrderClient dbOrderClient)
        {
            IPrintReport iprintReport = null;
            try
            {
                CrudAPI api = new CrudAPI(si.BaseAPI);
                var debtorInvoicePrint = new DebtorInvoicePrintReport(res, api, CompanyLayoutType.Invoice, dbOrderClient);
                long invoiceNumber = res.Header._InvoiceNumber;
                if (debtorInvoicePrint != null)
                {
                    await debtorInvoicePrint.InstantiateFields();
                    var standardDebtorInvoice = new DebtorInvoiceReportClient(debtorInvoicePrint.Company, debtorInvoicePrint.Debtor, debtorInvoicePrint.DebtorInvoice, debtorInvoicePrint.InvTransInvoiceLines, debtorInvoicePrint.DebtorOrder,
                        debtorInvoicePrint.DocumentClient.DocumentData, debtorInvoicePrint.ReportName, isCreditNote: debtorInvoicePrint.IsCreditNote, messageClient: debtorInvoicePrint.MessageClient);
                    var standardReports = new IDebtorStandardReport[1] { standardDebtorInvoice };

                    iprintReport = new StandardPrintReport(api, standardReports, (byte)StandardReports.Invoice);
                    await iprintReport.InitializePrint();
                }

                //If standard Report doesn't exist look for Layout
                if (iprintReport?.Report == null)
                {
                    iprintReport = new LayoutPrintReport(api, res, CompanyLayoutType.Invoice);
                    await iprintReport.InitializePrint();
                }

                if (iprintReport?.Report == null)
                {

                }

                var iReports = new IPrintReport[1] { iprintReport };
                var reportName = invoiceNumber > 0 ? string.Format("{0}_{1}", Localization.lookup("Invoice"), invoiceNumber) : Localization.lookup("Invoice");
                var dockName = invoiceNumber > 0 ? string.Format("{0}: {1}", Localization.lookup("Invoice"), invoiceNumber) : Localization.lookup("Invoice");


            }
            catch (Exception ex)
            {
                throw ex;
            }
            return iprintReport;
        }
        #endregion

        #region Others

        public override IComparer GridSorting(UnicontaBaseEntity obj)
        {
            if (obj is DebtorOrderLinePortal)
                return new DCOrderLineSort();
            else
                return base.GridSorting(obj);
        }

        #endregion
    }
}