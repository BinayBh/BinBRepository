var editingIndex = -1;
var insertSuccess = false;
var rowkey = null;
var redirectUrl = null;


$(document).on("click", "#btnCreateDebtor", function (e) {
    e.preventDefault();
    CreateDebtorAccounts();
})

$(document).on("click", "#btnCreateSalesOrder", function (e) {
    e.preventDefault();
    CreateSalesOrder();
})

$(document).on("click", "#btnCreateDebGroup", function (e) {
    e.preventDefault();
    CreateDebtorGroup();
})

$(document).on("click", "#btnCreateDebOffer", function (e) {
    e.preventDefault();
    CreateDebtorOffer();
})


function validateEmail(Email) {
    var pattern = /^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$/;

    return $.trim(Email).match(pattern) ? true : false;
}
//----------------------------------------------------------- Debtor Account section start

function CreateDebtorAccounts() {
    try {
        $.ajax({
            type: "GET",
            async: false,
            //traditional: true,
            contentType: 'application/json; charset=utf-8',
            url: "/DebtorAccount/Create",
            success: function (data) {
                $('#tabDebtorAccounts .dx-tabs-wrapper').html('');
                $('div.modal-footer').remove();
                $("#popupEditDebtorAccounts").dxPopup({ title: $(data).find("#hdnTitle").val() }).dxPopup("instance").show();
                var inst = $("#tabDebtorAccounts").dxTabPanel({
                }).dxTabPanel("instance");
                var items = inst.option("items");
                items.length = 0
                inst.option("items", items);
                $(data).find('div.tabbable ul  a').each(function () {
                    var link = $(this).attr('href');
                    var text = $(this).text();
                    items.push({ title: text, template: "<br/><br/>" + $(this).parentsUntil('form').find('div' + link).html() + "<br/><br/>" });
                })
                inst.option("items", items);
                inst.option("deferRendering", false);
                inst.option("selectedIndex", 0);
                inst.option("animationEnabled", true);
                $(data).find('div.modal-footer').appendTo($("#tabDebtorAccounts"));
                $('#EditDebtorForm').attr('action', $(data).find('#hdnActionName').val())
            },
            complete: function (response) {
                if (response.statusText === "Unauthorized") {
                    location.href = logOutUrl;
                }
            }
        });
    }
    catch (err) {
        Shownotification(error, err.message + err.name)
    }
}

function ShowHidePopupDebtorAccount(rowData, e) {
    var strUrl;
    try {
        switch (e) {
            case 'ViewAccount':
                strUrl = "/DebtorAccount/Details";
                break;
            case 'EditAccount':
                strUrl = "/DebtorAccount/Edit";
                break;
            case 'DeleteAccount':
                strUrl = "/DebtorAccount/Delete";
                break;
            default:
                strUrl = "/DebtorAccount/Details"
        }
        $.ajax({
            type: "GET",
            traditional: true,
            contentType: 'application/json; charset=utf-8',
            url: strUrl,
            data: { id: rowData.RowId },
            success: function (data) {
                $('#tabDebtorAccounts .dx-tabs-wrapper').html('');
                $('div.modal-footer').remove();
                $("#popupEditDebtorAccounts").dxPopup({ title: $(data).find("#hdnTitle").val() }).dxPopup("instance").show();
                var inst = $("#tabDebtorAccounts").dxTabPanel({
                }).dxTabPanel("instance");
                var items = inst.option("items");
                items.length = 0
                inst.option("items", items);
                $(data).find('div.tabbable ul  a').each(function () {
                    var link = $(this).attr('href');
                    var text = $(this).text();
                    items.push({ title: text, template: "<br/><br/>" + $(this).parentsUntil('form').find('div' + link).html() + "<br/><br/>" });
                })
                inst.option("items", items);
                inst.option("deferRendering", false);
                inst.option("selectedIndex", 0);
                inst.option("animationEnabled", true);
                $(data).find('div.modal-footer').appendTo($("#tabDebtorAccounts"));
                $('#EditDebtorForm').attr('action', $(data).find('#hdnActionName').val())
            },
            complete: function (response) {
                if (response.statusText === "Unauthorized") {
                    location.href = logOutUrl;
                }
            }
        });
    }
    catch (err) {
        Shownotification(error, err.message + err.name)
    }
}

function OnDebAcSuccess(data) {

    if (data.success == true) {
        $('#popupEditDebtorAccounts').dxPopup('instance').hide();
        Shownotification("success", data.message)
        $('#dxGridDebtorACcount').dxDataGrid('instance').refresh();
    }
    else if (data.success == false) {
        Shownotification("error", data.message)
    }
}

function OnDebAccFailure(data) {
    Shownotification("error", 'HTTP Status Code: ' + data.param1 + '  Error Message: ' + data.param2)
}

function tabDebAcc_OncontReady(args) {
    window.setTimeout(function () {
        var validators = args.element.find(".dx-validator")
        validators.each(function (index, elemmen) {
            var elemment = $(elemmen).dxValidator("instance");
            elemment.on("validated", function (e) {
                if (!e.isValid) {
                    var inst = $("#tabDebtorAccounts").dxTabPanel("instance");
                    var tabpanelItem = e.element.closest(".dx-multiview-item").data("dxMultiViewItemData");
                    inst.option("selectedItem", tabpanelItem);
                }
            });
        });

    }, 100);
}

//----------------------------------------------------------- Debtor Account section end



//----------------------------------------------------------- Debtor Order section start

function CreateSalesOrder() {
    try {
        $.ajax({
            type: "GET",
            traditional: true,
            contentType: 'application/json; charset=utf-8',
            url: "/DebtorAccount/CreateOrder",
            success: function (data) {
                $('#tabSalesOrder .dx-tabs-wrapper').html('');
                $('div.modal-footer').remove();
                $("#popupEditSalesOrder").dxPopup({ title: $(data).find("#hdnTitle").val() }).dxPopup("instance").show();
                var inst = $("#tabSalesOrder").dxTabPanel({
                }).dxTabPanel("instance");
                var items = inst.option("items");
                items.length = 0
                inst.option("items", items);
                $(data).find('div.tabbable ul  a').each(function () {
                    var link = $(this).attr('href');
                    var text = $(this).text();
                    items.push({ title: text, template: "<br/><br/>" + $(this).parentsUntil('form').find('div' + link).html() + "<br/><br/>" });
                })
                inst.option("items", items);
                inst.option("deferRendering", false);
                inst.option("selectedIndex", 0);
                inst.option("animationEnabled", true);
                $(data).find('div.modal-footer').appendTo($("#tabSalesOrder"));
                $('#EditSalesOrderForm').attr('action', $(data).find('#hdnActionName').val())
            },
            complete: function (response) {
                if (response.statusText === "Unauthorized") {
                    location.href = logOutUrl;
                }
            }
        });
    }
    catch (err) {
        Shownotification(error, err.message + err.name)
    }
}

function ShowHidePopupSalesOrder(rowData, e) {
    var strUrl;
    try {
        switch (e) {
            case 'ViewOrder':
                strUrl = "/DebtorAccount/OrderDetails";
                break;
            case 'EditOrder':
                strUrl = "/DebtorAccount/EditOrder";
                break;
            case 'DeleteOrder':
                strUrl = "/DebtorAccount/DeleteOrder";
                break;
            default:
                strUrl = "/DebtorAccount/OrderDetails"
        }
        $.ajax({
            type: "GET",
            traditional: true,
            contentType: 'application/json; charset=utf-8',
            url: strUrl,
            data: { id: rowData.RowId },
            success: function (data) {
                $('#tabSalesOrder .dx-tabs-wrapper').html('');
                $('div.modal-footer').remove();
                $("#popupEditSalesOrder").dxPopup({ title: $(data).find("#hdnTitle").val() }).dxPopup("instance").show();
                var inst = $("#tabSalesOrder").dxTabPanel({
                }).dxTabPanel("instance");
                var items = inst.option("items");
                items.length = 0
                inst.option("items", items);
                $(data).find('div.tabbable ul  a').each(function () {
                    var link = $(this).attr('href');
                    var text = $(this).text();
                    items.push({ title: text, template: "<br/><br/>" + $(this).parentsUntil('form').find('div' + link).html() + "<br/><br/>" });
                })
                inst.option("items", items);
                inst.option("deferRendering", false);
                inst.option("selectedIndex", 0);
                inst.option("animationEnabled", true);
                $(data).find('div.modal-footer').appendTo($("#tabSalesOrder"));
                $('#EditSalesOrderForm').attr('action', $(data).find('#hdnActionName').val())
            },
            complete: function (response) {
                if (response.statusText === "Unauthorized") {
                    location.href = logOutUrl;
                }
            }
        });
    }
    catch (err) {
        Shownotification(error, err.message + err.name)
    }
}

function setUserActionSaveNGL(data) {
    $('#btnUserAction').val("saveandgotoline");
}

function setUserActionSave(data) {
    //var buttonId = data.component.option("ID");
    $('#btnUserAction').val("save");
}

function OnDebtorOrderSuccess(data) {
    if (data.saveNGoToLine == true) {
        $('#popupEditSalesOrder').dxPopup('instance').hide();
        //Shownotification("success", data.message)
        //$('#dxGridDebtorOrders').dxDataGrid('instance').refresh();
        var url = '/DebtorAccount/DebtorOrderLines?accNo=' + data.accNo + '&' + 'orderNo=' + data.orderNo;
        window.location.href = url;
    }
    else if (data.success == true) {
        $('#popupEditSalesOrder').dxPopup('instance').hide();
        Shownotification("success", data.message)
        window.setTimeout(function () {
            $('#dxGridDebtorOrders').dxDataGrid('instance').refresh();
        }, 200);
    }
    else if (data.success == false) {
        Shownotification("error", data.message)
    }
    else if (data.success === 'UpdateDocTrue') {
        $('#popupEditSalesOrderDoc').dxPopup('instance').hide();
        if (data.message) {
            Shownotification("success", data.message);
        }

        window.setTimeout(function () {
            var grid = $('#dxGridDebtorOrders').dxDataGrid('instance')
            if (grid)
                grid.refresh();
            if (data.ShowInvoice == true)
                window.open("/DebtorAccount/ShowInvoice?" + data.PrintPreviewParams);
            if (data.ShowSimulatedTransactions == true)
                window.open("/DebtorAccount/SimulatedTransactions?" + data.PrintPreviewParams);

        }, 2000);


    }
}

function OnDebOrdFailure(data) {
    Shownotification("error", 'HTTP Status Code: ' + data.param1 + '  Error Message: ' + data.param2)
}

function tabDebOrd_OncontReady(args) {
    window.setTimeout(function () {
        var validators = args.element.find(".dx-validator")
        validators.each(function (index, elemmen) {
            var elemment = $(elemmen).dxValidator("instance");
            elemment.on("validated", function (e) {
                if (!e.isValid) {
                    var inst = $("#tabSalesOrder").dxTabPanel("instance");
                    var tabpanelItem = e.element.closest(".dx-multiview-item").data("dxMultiViewItemData");
                    inst.option("selectedItem", tabpanelItem);
                }
            });
        });

    }, 100);
}

//----------------------------------------------------------- Debtor Order section end



//----------------------------------------------------------- Debtor Order line section start

//Only show those col for edit having visible true.
function dxGridDebOrderLines_OnEditingStart(e) {
    editingIndex = e.component.getRowIndexByKey(e.key);
    var grid = $("#dxGridDebtorOrderLine").dxDataGrid('instance');
    manageDebCellVisibility(grid);
}

function insertNewDebOrderLine() {
    var grid = $('#dxGridDebtorOrderLine').dxDataGrid('instance');
    manageDebCellVisibility(grid);
    grid.addRow();
}


function insertNewDebOrderLine1() {
    var grid = $('#dxGridDebtorOrderLine').dxDataGrid('instance');
    manageDebCellVisibility(grid);
    rowkey = grid.getSelectedRowKeys()[0];

    if (rowkey) {
        editingIndex = grid.getRowIndexByKey(rowkey);
        grid.addRow(rowkey);
    }
    else {
        grid.addRow();
    }
}

function manageDebCellVisibility(grid) {
    for (i = 0; i < grid.columnCount(); i++) {
        var formItem = grid.columnOption(i).formItem;
        if (grid.columnOption(i, 'visible') && formItem) {
            formItem.visible = true;
        }
        else if (!grid.columnOption(i, 'visible') && formItem) {
            formItem.visible = false;
        }
    }
}

//Code to disable chield lookup when parent lookup is empty or not selected.
function dxGridDebOrderLines_onEditorPreparing(e) {
    if (e.row != null && e.row != "") {
        if (!e.row.data.Item) {
            if (e.dataField === "Variant1" || e.dataField === "Variant2" || e.dataField === "SerieBatch") {
                e.editorOptions.disabled = true;
                e.editorOptions.value = null;
            }
        }
        if ((e.row.data.Item) && (!e.row.data.Variant1)) {
            if (e.dataField === "Variant2") {
                e.editorOptions.disabled = true;
                e.editorOptions.value = null;
            }
        }
        if (!e.row.data.Warehouse) {
            if (e.dataField === "Location") {
                e.editorOptions.disabled = true;
                e.editorOptions.value = null;
            }
        }
    }
    if (e.parentType === "dataRow" && (e.dataField == "Item" || e.dataField == "Qty" || e.dataField == "Subtotal" || e.dataField == "Total" || e.dataField == "EAN")) {
        var standardHandler = e.editorOptions.onValueChanged;
        var rowId = e.row.data.RowId;
        e.editorOptions.onValueChanged = function (args) {
            $.getJSON('/DebtorAccount/setDependedField', { 'value': args.value, 'propertyName': e.dataField, 'id': rowId }, function (data, status) {
            }).success(function (data) {
                if (data == '' || data == null) {
                    alert("data not found");
                }
                else {
                    if (data) {
                        var resultData = JSON.parse(data.result)
                        //var resultData = JSON.parse(data)
                        e.component.beginUpdate();
                        e.component.cellValue(e.row.rowIndex, "Item", resultData.Item);
                        e.component.cellValue(e.row.rowIndex, "Subtotal", resultData.Subtotal);
                        //e.component.cellValue(e.row.rowIndex, "Note", resultData.Note);
                        e.component.cellValue(e.row.rowIndex, "Variant1", resultData.Variant1);
                        e.component.cellValue(e.row.rowIndex, "Variant2", resultData.Variant2);
                        e.component.cellValue(e.row.rowIndex, "Text", resultData.Text);
                        e.component.cellValue(e.row.rowIndex, "Qty", resultData.Qty);
                        e.component.cellValue(e.row.rowIndex, "SerieBatch", resultData.SerieBatch);
                        //e.component.cellValue(e.row.rowIndex, "EAN", resultData.EAN);
                        e.component.cellValue(e.row.rowIndex, "Price", resultData.Price);
                        //e.component.cellValue(e.row.rowIndex, "DiscountPct", resultData.DiscountPct);
                        e.component.cellValue(e.row.rowIndex, "Total", resultData.Total);
                        e.component.cellValue(e.row.rowIndex, "Margin", resultData.Margin);
                        e.component.cellValue(e.row.rowIndex, "SalesValue", resultData.SalesValue);
                        e.component.cellValue(e.row.rowIndex, "CostValue", resultData.CostValue);
                        e.component.cellValue(e.row.rowIndex, "MarginRatio", resultData.MarginRatio);
                        e.component.cellValue(e.row.rowIndex, "DoInvoice", resultData.DoInvoice);
                        //e.component.cellValue(e.row.rowIndex, "QtyNow", resultData.QtyNow);
                        //e.component.cellValue(e.row.rowIndex, "QtyDelivered", resultData.QtyDelivered);
                        e.component.cellValue(e.row.rowIndex, "QtyInvoiced", resultData.QtyInvoiced);
                        e.component.cellValue(e.row.rowIndex, "Currency", resultData.Currency);
                        e.component.cellValue(e.row.rowIndex, "CostPrice", resultData.CostPrice);
                        e.component.cellValue(e.row.rowIndex, "Warehouse", resultData.Warehouse);
                        e.component.cellValue(e.row.rowIndex, "Location", resultData.Location);
                        e.component.cellValue(e.row.rowIndex, "Storage", resultData.Storage);
                        //e.component.cellValue(e.row.rowIndex, "IgnoreBlocked", resultData.IgnoreBlocked);
                        //e.component.cellValue(e.row.rowIndex, "Week", resultData.Week);
                        //e.component.cellValue(e.row.rowIndex, "Date", resultData.Date);
                        e.component.cellValue(e.row.rowIndex, "Unit", resultData.Unit);
                        //e.component.cellValue(e.row.rowIndex, "Project", resultData.Project);
                        e.component.cellValue(e.row.rowIndex, "PrCategory", resultData.PrCategory);
                        //e.component.cellValue(e.row.rowIndex, "PostingAccount", resultData.PostingAccount);
                        e.component.cellValue(e.row.rowIndex, "Vat", resultData.Vat);
                        e.component.cellValue(e.row.rowIndex, "Employee", resultData.Employee);
                        //e.component.cellValue(e.row.rowIndex, "Withholding", resultData.Withholding);
                        e.component.cellValue(e.row.rowIndex, "Dimension1", resultData.Dimension1);
                        e.component.cellValue(e.row.rowIndex, "Dimension2", resultData.Dimension2);
                        e.component.cellValue(e.row.rowIndex, "Dimension3", resultData.Dimension3);
                        e.component.cellValue(e.row.rowIndex, "Dimension4", resultData.Dimension4);
                        e.component.cellValue(e.row.rowIndex, "Dimension5", resultData.Dimension5);
                        e.component.endUpdate();
                    }
                }
            });
            standardHandler(args);
            //.fail(function (response) {
            //});
        }
    }
}


function dxGridDebOrderLines_OnDataErrorOccurred(e) {
    Shownotification(e.error.name, e.error);
}

function dxGridDebOrderLines_OnInitNewRow(e) {
    e.data.Date = new Date();
}

function btnDeleteOrderLine_Click() {
    var grid = $("#dxGridDebtorOrderLine").dxDataGrid("instance");
    grid.deleteRow(editingIndex);
}

function btnSaveOrderLine_Click(e) {
    var grid = $("#dxGridDebtorOrderLine").dxDataGrid("instance");
    grid.saveEditData();
}

function btnCancelOrderLine_Click(e) {
    var grid = $("#dxGridDebtorOrderLine").dxDataGrid("instance");
    grid.cancelEditData();
}

// Start script section to manage "save and move next"/"save and move prev" 
function btnDebOrdSaveAndPrev_Click() {
    var grid = $("#dxGridDebtorOrderLine").dxDataGrid("instance");
    grid.saveEditData().done(function () {
        if (insertSuccess) {
            grid.addRow(rowkey);
            insertSuccess = false;
        }
        else {
            ManageGridPagingBack(grid);
        }
    })
}

function btnDebOrdSaveAndNext_Click() {

    var grid = $("#dxGridDebtorOrderLine").dxDataGrid("instance");
    grid.saveEditData().done(function () {
        if (insertSuccess) {
            grid.addRow(rowkey);
            insertSuccess = false;
        }
        else {
            ManageGridPagingFwd(grid);
        }
    })
}

function ManageGridPagingFwd(grid) {
    var pageIndex = grid.pageIndex()
    var pageCount = grid.pageCount()
    //var pageSize = grid.pageSize()
    var moveNext = false;
    if ((editingIndex == grid.getVisibleRows().length - 1) && (pageIndex < pageCount - 1)) {
        editingIndex = -1;
        $.when(grid.pageIndex(pageIndex + 1)).done(function () {
            grid.editRow(editingIndex + 1);
            moveNext = true;
        });

    }
    else if (editingIndex < grid.getVisibleRows().length - 1) {
        grid.editRow(editingIndex + 1);
        moveNext = true;
    }
    return moveNext;
}

function ManageGridPagingBack(grid) {
    var pageIndex = grid.pageIndex()
    var pageCount = grid.pageCount()
    //var pageSize = grid.pageSize()
    var moveNext = false;
    if ((editingIndex == 0) && (pageIndex > 0)) {

        $.when(grid.pageIndex(pageIndex - 1)).done(function () {
            editingIndex = grid.getVisibleRows().length;
            grid.editRow(editingIndex - 1);
            moveNext = true;
        });

    }
    else if (editingIndex > 0) {
        grid.editRow(editingIndex - 1);
        moveNext = true;
    }
    return moveNext;
}
// End script section

var warehouse = null;
var propName = 'Warehouse';
var item = null;
var qty = null;
var price = null;
var variant1 = null;
var itemPropName = 'Item';
var rowId = null;

function dxGridDebOrderLines_OnContentReady(e) {

    isGridRefreshed = false;
    var grid = e.component;
    var selection = grid.getSelectedRowKeys();
    if (selection.length == 0) {
        grid.selectRowsByIndexes([0]);
    }
    var el = e.component.element().find('.dx-page-size').last();
    el.text('All');
    el.click(function () {
        e.component.pageSize(0);
        el.text('All');
    });
    item = null;
    variant1 = null;
    warehouse = null;
    rowId = null;
}

function setWarehouse(rowData, value) {
    rowData.Location = null;
    this.defaultSetCellValue(rowData, value);
    warehouse = value;
}

function getLocations(option) {
    if (option.data) {
        warehouse = option.data.Warehouse;
    }
    var dataSourceConfiguration = {
        store: DevExpress.data.AspNet.createStore({
            key: "KeyStr",
            loadUrl: '/DebtorAccount/DebtorOrderLineGrid_PropertyChanged',
            loadParams: { 'propertyName': propName, 'value': warehouse }
        })
    };
    return dataSourceConfiguration;
}

function setItem(rowData, value) {
    rowData.Variant1 = null;
    rowData.Variant2 = null;
    rowData.SerieBatch = null;
    this.defaultSetCellValue(rowData, value);
    item = value;
    rowId = rowData.RowId;
}


function getVariants1(option) {
    //fill first variant here.
    if (option.data) {
        item = option.data.Item;
    }
    var dataSourceConfiguration = {
        store: DevExpress.data.AspNet.createStore({
            key: "KeyStr",
            loadUrl: '/DebtorAccount/DebtorOrderLineGrid_PropertyChanged',
            loadParams: { 'propertyName': itemPropName, 'value': item }
        })
    };
    return dataSourceConfiguration;
}

function setVariant1(rowData, value) {
    rowData.Variant2 = null;
    this.defaultSetCellValue(rowData, value);
    variant1 = value;
}

function getVariants2(option) {
    if (option.data) {
        item = option.data.Item;
        variant1 = option.data.Variant1;
    }
    var dataSourceConfiguration = {
        store: DevExpress.data.AspNet.createStore({
            key: "KeyStr",
            loadUrl: '/DebtorAccount/setVariant',
            loadParams: { 'propertyName': itemPropName, 'value': item, 'SetVariant2': true, 'vr1': variant1 }
        })
    };
    return dataSourceConfiguration;
}

function getSerieBatch(option) {
    if (option.data) {
        item = option.data.Item;
        rowId = option.data.RowId;
    }

    var dataSourceConfiguration = {
        store: DevExpress.data.AspNet.createStore({
            key: "KeyStr",
            loadUrl: '/DebtorAccount/setSerieBatch',
            loadParams: { 'value': item, 'rowId': rowId }
        })
    };
    return dataSourceConfiguration;
}

function GenerateInvoiceDebtorLine(e) {
    var grid = $('#dxGridDebtorOrderLine').dxDataGrid('instance')
    if (grid.hasEditData()) {
        grid.saveEditData().done(function () {

            GenrateInvoiceLocalSave();
        });
    }
    else {
        grid.cancelEditData();
        GenrateInvoiceLocalSave();
    }
}

function GenrateInvoiceLocalSave() {
    try {

        $.ajax({
            type: "GET",
            async: false,
            cache: false,
            contentType: 'application/json; charset=utf-8',
            url: "/DebtorAccount/GenerateInvoiceOrderLine",
            success: function (data) {

            },
            complete: function (response) {

                if (response.statusText === "Unauthorized") {
                    location.href = logOutUrl;
                }
                else if (response.statusText === "OK") {
                    if (response.responseJSON.IsValid === false) {
                        function beforeDisassociate() {
                            var returnValue = new $.Deferred();
                            var result = DevExpress.ui.dialog.confirm(response.responseJSON.message, response.responseJSON.title);
                            result.done(function (dialogResult) {
                                returnValue.resolve(dialogResult);
                            });
                            return returnValue.promise();
                        }
                        beforeDisassociate().done(function (result) {
                            if (result === true) {
                                DebtorUpdateDocument("Invoice", response.responseJSON.RowId);
                            }
                        });

                    }
                    else {
                        DebtorUpdateDocument("Invoice", response.responseJSON.RowId);
                    }
                }
            }
        });
    }
    catch (err) {
        Shownotification(error, err.message + err.name)
    }
}


//----------------------------------------------------------- Debtor Order line section end



//----------------------------------------------------------- Debtor offers section start

function CreateDebtorOffer() {
    try {
        $.ajax({
            type: "GET",
            traditional: true,
            cache: false,
            contentType: 'application/json; charset=utf-8',
            url: "/DebtorAccount/CreateOffer",
            success: function (data) {
                $('#tabDebtorOffers .dx-tabs-wrapper').html('');
                $('div.modal-footer').remove();
                $("#popupEditDebtorOffers").dxPopup({ title: $(data).find("#hdnTitle").val() }).dxPopup("instance").show();
                var inst = $("#tabDebtorOffers").dxTabPanel({
                }).dxTabPanel("instance");
                var items = inst.option("items");
                items.length = 0
                inst.option("items", items);
                $(data).find('div.tabbable ul  a').each(function () {
                    var link = $(this).attr('href');
                    var text = $(this).text();
                    items.push({ title: text, template: "<br/><br/>" + $(this).parentsUntil('form').find('div' + link).html() + "<br/><br/>" });
                })
                inst.option("items", items);
                inst.option("deferRendering", false);
                inst.option("selectedIndex", 0);
                inst.option("animationEnabled", true);
                $(data).find('div.modal-footer').appendTo($("#tabDebtorOffers"));
                $('#EditDebOffersForm').attr('action', $(data).find('#hdnActionName').val())
            },
            complete: function (response) {
                if (response.statusText === "Unauthorized") {
                    location.href = logOutUrl;
                }
            }
        });
    }
    catch (err) {
        Shownotification(error, err.message + err.name)
    }
}

function ShowHidePopupDebtorOffers(rowData, e) {
    var strUrl;
    try {
        switch (e) {
            case 'ViewDebOffer':
                strUrl = "/DebtorAccount/DebOfferDetails";
                break;
            case 'EditDebOffer':
                strUrl = "/DebtorAccount/EditDebOffer";
                break;
            case 'DeleteDebOffer':
                strUrl = "/DebtorAccount/DeleteDebOffer";
                break;
            default:
                strUrl = "/DebtorAccount/DebOfferDetails"
        }
        $.ajax({
            type: "GET",
            traditional: true,
            contentType: 'application/json; charset=utf-8',
            url: strUrl,
            cache: false,
            data: { id: rowData.RowId },
            success: function (data) {
                $('#tabDebtorOffers .dx-tabs-wrapper').html('');
                $('div.modal-footer').remove();
                $("#popupEditDebtorOffers").dxPopup({ title: $(data).find("#hdnTitle").val() }).dxPopup("instance").show();
                var inst = $("#tabDebtorOffers").dxTabPanel({
                }).dxTabPanel("instance");
                var items = inst.option("items");
                items.length = 0
                inst.option("items", items);
                $(data).find('div.tabbable ul  a').each(function () {
                    var link = $(this).attr('href');
                    var text = $(this).text();
                    items.push({ title: text, template: "<br/><br/>" + $(this).parentsUntil('form').find('div' + link).html() + "<br/><br/>" });
                })
                inst.option("items", items);
                inst.option("deferRendering", false);
                inst.option("selectedIndex", 0);
                inst.option("animationEnabled", true);
                $(data).find('div.modal-footer').appendTo($("#tabDebtorOffers"));
                $('#EditDebOffersForm').attr('action', $(data).find('#hdnActionName').val())
            },
            complete: function (response) {
                if (response.statusText === "Unauthorized") {
                    location.href = logOutUrl;
                }
            }
        });
    }
    catch (err) {
        Shownotification(error, err.message + err.name)
    }
}

function setUserActionSaveDebOfferNGL(data) {
    $('#btnUserAction').val("saveandgotoline");
}

function setUserActionSaveDebOffer(data) {
    $('#btnUserAction').val("save");
}

function OnDebtorOffersSuccess(data) {
    if (data.saveNGoToLine == true) {
        $('#popupEditDebtorOffers').dxPopup('instance').hide();
        var url = '/DebtorAccount/DebtorOfferLines?rowID=' + data.rowId;
        window.location.href = url;
    }
    else if (data.success == true) {
        $('#popupEditDebtorOffers').dxPopup('instance').hide();
        Shownotification("success", data.message)
        window.setTimeout(function () {
            $('#dxGridDebtorOffers').dxDataGrid('instance').refresh();
        }, 200);
    }
    //else if (data.success === 'CreateSPOrd') {
    //    $('#popupCreateSalesPurchaseOrder').dxPopup('instance').hide();
    //    confirmationForGoToLine(data);
    //    Shownotification("success", data.message)
    //    window.setTimeout(function () {
    //        $('#dxGridDebtorOffers').dxDataGrid('instance').refresh();
    //    }, 200);
    //}
    else if (data.success == false) {
        Shownotification("error", data.message);
    }

}

function OnDebtorOffersFailure(data) {
    Shownotification("error", 'HTTP Status Code: ' + data.param1 + '  Error Message: ' + data.param2);
}

function OnCreateOrderByQuotationSuccess(data) {
    if (data.success === 'CreateSPOrd') {
        $('#popupCreateSalesPurchaseOrder').dxPopup('instance').hide();
        confirmationForGoToLine(data);
        Shownotification("success", data.message)
        window.setTimeout(function () {
            $('#dxGridDebtorOffers').dxDataGrid('instance').refresh();
        }, 200);
    }
    else if (data.success == false) {
        Shownotification("error", data.message);
    }
}

function OnCreateOrderByQuotationFailure(data) {
    Shownotification("error", 'HTTP Status Code: ' + data.param1 + '  Error Message: ' + data.param2);
}

function tabDebOffers_OncontReady(args) {
    window.setTimeout(function () {
        var validators = args.element.find(".dx-validator")
        validators.each(function (index, elemmen) {
            var elemment = $(elemmen).dxValidator("instance");
            elemment.on("validated", function (e) {
                if (!e.isValid) {
                    var inst = $("#tabDebtorOffers").dxTabPanel("instance");
                    var tabpanelItem = e.element.closest(".dx-multiview-item").data("dxMultiViewItemData");
                    inst.option("selectedItem", tabpanelItem);
                }
            });
        });

    }, 100);
}

// Code to convert quotation to debtor order.
function ConvertQuotationToOrder() {
    var strUrl = "/DebtorAccount/ConvertQuotToOrder";
    var grid = $('#dxGridDebtorOffers').dxDataGrid('instance');
    rowkey = grid.getSelectedRowKeys()[0];
    if (rowkey) {
        try {
            $.ajax({
                type: "GET",
                async: false,
                cache: false,
                contentType: 'application/json; charset=utf-8',
                url: strUrl,
                data: { rowId: rowkey },
                success: function (data) {
                    if (data.success == true) {
                        Shownotification("success", data.message)
                        window.setTimeout(function () {
                            RefreshDxDataGrid('dxGridDebtorOffers');
                        }, 200);
                    }
                },
                complete: function (response) {

                    if (response.statusText === "Unauthorized") {
                        location.href = logOutUrl;
                    }
                }
            });
        }
        catch (err) {
            Shownotification(error, err.message + err.name)
        }
    }
    else { Shownotification(error, GetResourceValue('NoSettlementSelected')); return false; }
}

//Code to create sales/purchase order from quotation popup
function CreateSalePurchaseOrder(rowId) {
    var strUrl = "/DebtorAccount/CreateSalePurchaseOrder";
    var grid = $('#dxGridDebtorOffers').dxDataGrid('instance');
    if (grid) {
        rowkey = grid.getSelectedRowKeys()[0];
    }
    else {
        rowkey = rowId;
    }
    if (rowkey) {
        try {
            $.ajax({
                type: "GET",
                async: false,
                cache: false,
                contentType: 'application/json; charset=utf-8',
                url: strUrl,
                data: { rowId: rowkey },
                success: function (data) {
                    var popup = $("#popupCreateSalesPurchaseOrder").dxPopup({ title: $(data).find("#hdnTitle").val() }).dxPopup("instance");
                    var currentScreen = screenByWidth($(window).width());
                    if (currentScreen === 'lg') {
                        popup.option('height', 450)
                        popup.option('width', 500)
                    }
                    popup.content().empty(); $(data).appendTo(popup.content());
                    popup.show();
                    //$('#EditOfferSalesPurchaseOrder').attr('action', $(data).find('#hdnActionName').val()).attr('data-ajax-failure', 'OnDebtorOffersFailure(data)').attr('data-ajax-success', 'OnDebtorOffersSuccess')
                    $('#EditOfferSalesPurchaseOrder').attr('action', $(data).find('#hdnActionName').val()).attr('data-ajax-failure', 'OnCreateOrderByQuotationFailure(data)').attr('data-ajax-success', 'OnCreateOrderByQuotationSuccess')
                },
                complete: function (response) {
                    if (response.statusText === "Unauthorized") {
                        location.href = logOutUrl;
                    }
                }
            });
        }
        catch (err) {
            Shownotification(error, err.message + err.name)
        }
    }
    else { Shownotification(error, GetResourceValue('NoSettlementSelected')); return false; }
}

function CloseCreateSPOPopUp() {
    $('#popupCreateSalesPurchaseOrder').dxPopup('instance').hide();
}

//Submit the form if the selected account has not been blocked.
//function ValidityCheck_Click() {
//    manageOfferFields();
//    $("#dxOfferSPO").submit();
//    $('#popupCheckValidity').dxPopup('instance').hide();
//}

function setFrmHdnFields() {
    manageOfferFields();
}

function manageOfferFields() {
    var value = findSelectedIndex();
    $("#hdnSelcIndex").attr("value", value);
    if (value == 1) {
        $("#hdnIsDeb").attr("value", false);
    }
    else {
        $("#hdnIsDeb").attr("value", true);
    }
}

//Close the popup and denied submition of form if the selected account has not been blocked and user don't want to continue.
//function CloseChkValidationPopUp() {
//    $('#popupCheckValidity').dxPopup('instance').hide();
//}

// This popup is used to show the detail of selected account i.e. selected account is blocked or not.
function CheckEligibilityForCreateSPOrd() {
    var value = findSelectedIndex();
    var account = $("#sbDCAccount").dxSelectBox("instance").option("value");
    var strUrl = "/DebtorAccount/checkValidity";
    $.ajax({
        type: "GET",
        async: false,
        cache: false,
        contentType: 'application/json; charset=utf-8',
        url: strUrl,
        data: { value: value, account: account },
        success: function (data) {
            if (data.success == "AccountBlocked") {
                //var popup = $("#popupCheckValidity").dxPopup("instance");
                //popup.option("contentTemplate", $("#popupValidityTemplate"));
                //popup.show();
                //response.responseJSON.message, response.responseJSON.title
                function beforeDisassociate() {
                        var returnValue = new $.Deferred();
                        var result = DevExpress.ui.dialog.confirm(data.message, data.title);
                        result.done(function (dialogResult) {
                            returnValue.resolve(dialogResult);
                        });
                        return returnValue.promise();
                    }
                    beforeDisassociate().done(function (result) {
                        if (result === true) {
                            manageOfferFields();
                            $("#dxOfferSPO").submit();
                        }
                    });
            }
            else if (data.success == "ValidAccount") {
                $("#dxOfferSPO").submit();
            }
        },
        complete: function (response) {

            if (response.statusText === "Unauthorized") {
                location.href = logOutUrl;
            }
        }
    });
}


function confirmationForGoToLine(data) {
    var selectedIndex = data.sIndex;
    var msg = data.cMsg;

    if (selectedIndex ==0)
        redirectUrl = '/DebtorAccount/DebtorOrderLines?accNo=' + data.account + '&' + 'orderNo=' + data.ordNum;
    else if (selectedIndex == 1)
        redirectUrl = '/CreditorAccount/CreditorOrderLines?accNo=' + data.account + '&' + 'orderNo=' + data.ordNum;
    else if (selectedIndex == 2)
        redirectUrl = '/DebtorAccount/DebtorOfferLines?rowId=' + data.rowId;

    //var popup = $("#popupGoToLineConfirmation").dxPopup("instance");
    //popup.option("contentTemplate", $("#popupConfirmationTemplate"));
    //popup.show();
    //$("#cnfrmText").text(msg);
    function beforeDisassociate() {
        var returnValue = new $.Deferred();
        var result = DevExpress.ui.dialog.confirm(data.cMsg, data.title);
        result.done(function (dialogResult) {
            returnValue.resolve(dialogResult);
        });
        return returnValue.promise();
    }
    beforeDisassociate().done(function (result) {
        if (result === true) {
            window.location.href = redirectUrl;
        }
    });
}

//function FireActionByType() {
//    window.location.href = redirectUrl;
//    $("#popupGoToLineConfirmation").dxPopup("instance").hide();
//}

//function CloseConfirmationPopUp() {
//    $("#popupGoToLineConfirmation").dxPopup("instance").hide();
//}

function cmbDCTInitialized(e) {
    var selectBox = $("#cmbDCTypeAcc").dxSelectBox("instance");
    var defaultVal = selectBox.option().dataSource.store._array[0];
    selectBox.option('value', defaultVal);
}

function setAccountsByDCType(e) {
    var sb = $("#sbDCAccount").dxSelectBox("instance");
    var value = findSelectedIndex();
    $("#hdnSelcIndex").attr("value", value);
    if (value == 1) {
        $("#ordPerchase").css("display", "block");
        $("#hdnIsDeb").attr("value", false);
    }
    else {
        $("#ordPerchase").css("display", "none");
        $("#hdnIsDeb").attr("value", true);
    }
    sb.getDataSource().reload();
    sb.reset();
}

function findSelectedIndex() {
    var cmbBox = $("#cmbDCTypeAcc").dxSelectBox("instance");
    var cmbVal = cmbBox.option("selectedItem");
    var value = cmbBox.option().dataSource.store._array.indexOf(cmbVal);
    return value;
}

function takeDCTypeForAccount(e) {
    var cmbBox = $("#cmbDCTypeAcc").dxSelectBox("instance");
    var sb = cmbBox.option("selectedItem");
    var value = cmbBox.option().dataSource.store._array.indexOf(sb);
    if (value >= 0)
        return value;
    else
        return "-1";
}
//----------------------------------------------------------- Debtor offers section end




//----------------------------------------------------------- Debtor Offer line section start

//Only show those col for edit having visible true.
function dxGridDebOfferLines_OnEditingStart(e) {
    editingIndex = e.component.getRowIndexByKey(e.key);
    var grid = $("#dxGridDebtorOfferLine").dxDataGrid('instance');
    manageDebCellVisibility(grid);
}

function insertNewDebOfferLine() {
    var grid = $('#dxGridDebtorOfferLine').dxDataGrid('instance');
    manageDebCellVisibility(grid);
    grid.addRow();
}

function insertNewDebOfferLine1() {
    var grid = $('#dxGridDebtorOfferLine').dxDataGrid('instance');
    manageDebCellVisibility(grid);
    rowkey = grid.getSelectedRowKeys()[0];

    if (rowkey) {
        editingIndex = grid.getRowIndexByKey(rowkey);
        grid.addRow(rowkey);
    }
    else {
        grid.addRow();
    }
}

//Code to disable chield lookup when parent lookup is empty or not selected.
function dxGridDebOfferLines_onEditorPreparing(e) {
    if (e.row != null && e.row != "") {
        if (!e.row.data.Item) {
            if (e.dataField === "Variant1" || e.dataField === "Variant2") {
                e.editorOptions.disabled = true;
                e.editorOptions.value = null;
            }
        }
        if ((e.row.data.Item) && (!e.row.data.Variant1)) {
            if (e.dataField === "Variant2") {
                e.editorOptions.disabled = true;
                e.editorOptions.value = null;
            }
        }
    }
    if (e.parentType === "dataRow" && (e.dataField == "Item" || e.dataField == "Qty" || e.dataField == "Subtotal" || e.dataField == "Total" || e.dataField == "EAN")) {
        var standardHandler = e.editorOptions.onValueChanged;
        var rowId = e.row.data.RowId;
        e.editorOptions.onValueChanged = function (args) {
            $.getJSON('/DebtorAccount/setDebOfferFields', { 'value': args.value, 'propertyName': e.dataField, 'id': rowId }, function (data, status) {
            }).success(function (data) {
                if (data == '' || data == null) {
                    alert("data not found");
                }
                else {
                    if (data) {
                        var resultData = JSON.parse(data.result)
                        e.component.beginUpdate();
                        e.component.cellValue(e.row.rowIndex, "Item", resultData.Item);
                        e.component.cellValue(e.row.rowIndex, "Subtotal", resultData.Subtotal);
                        e.component.cellValue(e.row.rowIndex, "Variant1", resultData.Variant1);
                        e.component.cellValue(e.row.rowIndex, "Variant2", resultData.Variant2);
                        e.component.cellValue(e.row.rowIndex, "Text", resultData.Text);
                        e.component.cellValue(e.row.rowIndex, "Qty", resultData.Qty);
                        e.component.cellValue(e.row.rowIndex, "Price", resultData.Price);
                        e.component.cellValue(e.row.rowIndex, "Total", resultData.Total);
                        e.component.cellValue(e.row.rowIndex, "Margin", resultData.Margin);
                        e.component.cellValue(e.row.rowIndex, "SalesValue", resultData.SalesValue);
                        e.component.cellValue(e.row.rowIndex, "CostValue", resultData.CostValue);
                        e.component.cellValue(e.row.rowIndex, "MarginRatio", resultData.MarginRatio);
                        e.component.cellValue(e.row.rowIndex, "DoInvoice", resultData.DoInvoice);
                        e.component.cellValue(e.row.rowIndex, "Currency", resultData.Currency);
                        e.component.cellValue(e.row.rowIndex, "CostPrice", resultData.CostPrice);
                        e.component.cellValue(e.row.rowIndex, "Unit", resultData.Unit);
                        e.component.cellValue(e.row.rowIndex, "PrCategory", resultData.PrCategory);
                        e.component.cellValue(e.row.rowIndex, "Vat", resultData.Vat);
                        e.component.cellValue(e.row.rowIndex, "Employee", resultData.Employee);
                        e.component.cellValue(e.row.rowIndex, "Dimension1", resultData.Dimension1);
                        e.component.cellValue(e.row.rowIndex, "Dimension2", resultData.Dimension2);
                        e.component.cellValue(e.row.rowIndex, "Dimension3", resultData.Dimension3);
                        e.component.cellValue(e.row.rowIndex, "Dimension4", resultData.Dimension4);
                        e.component.cellValue(e.row.rowIndex, "Dimension5", resultData.Dimension5);
                        e.component.endUpdate();
                    }
                }
            });
            standardHandler(args);
            //.fail(function (response) {
            //});
        }
    }
}

function dxGridDebOfferLines_OnDataErrorOccurred(e) {
    Shownotification(e.error.name, e.error);
}

function dxGridDebOfferLines_OnInitNewRow(e) {
    e.data.Date = new Date();
}

function btnDeleteOfferLine_Click() {
    var grid = $("#dxGridDebtorOfferLine").dxDataGrid("instance");
    grid.deleteRow(editingIndex);
}

function btnSaveOfferLine_Click(e) {
    var grid = $("#dxGridDebtorOfferLine").dxDataGrid("instance");
    grid.saveEditData();
}

function btnCancelOfferLine_Click(e) {
    var grid = $("#dxGridDebtorOfferLine").dxDataGrid("instance");
    grid.cancelEditData();
}

function btnDebOfferSaveAndPrev_Click() {
    var grid = $("#dxGridDebtorOfferLine").dxDataGrid("instance");
    grid.saveEditData().done(function () {
        if (insertSuccess) {
            grid.addRow(rowkey);
            insertSuccess = false;
        }
        else {
            ManageGridPagingBack(grid);
        }
    })
}

function btnDebOfferSaveAndNext_Click() {

    var grid = $("#dxGridDebtorOfferLine").dxDataGrid("instance");
    grid.saveEditData().done(function () {
        if (insertSuccess) {
            grid.addRow(rowkey);
            insertSuccess = false;
        }
        else {
            ManageGridPagingFwd(grid);
        }
    })
}

function GenerateInvoiceDebtorOfferLine(e) {
    var grid = $('#dxGridDebtorOfferLine').dxDataGrid('instance')
    if (grid.hasEditData()) {
        grid.saveEditData().done(function () {

            GenrateInvoiceLocalSave();
        });
    }
    else {
        grid.cancelEditData();
        GenrateInvoiceLocalSave();
    }
}

//To see the total inventory storage for quotaiton
function ViewOnHand() {
    var rowkey = null;
    var grid = $('#dxGridDebtorOfferLine').dxDataGrid('instance');
    if (grid) {
        rowkey = grid.getSelectedRowKeys()[0];
    }

    if (rowkey) {
        try {

            $.ajax({
                type: "GET",
                async: false,
                cache: false,
                contentType: 'application/json; charset=utf-8',
                url: "/DebtorAccount/Storage",
                data: { rowId: rowkey },
                success: function (data) {
                    var popup = $("#popupOfferStorageDetail").dxPopup({ title: $(data).filter("#hdnTitle2").val() }).dxPopup("instance");
                    popup.content().empty();
                    var scroolablediv = $("<div id='scrollView'>");
                    $(data).appendTo(scroolablediv);
                    $(scroolablediv).appendTo(popup.content());
                    $("#scrollView").dxScrollView({
                        direction: 'both'
                    });
                    popup.show();
                },
                complete: function (response) {
                    if (response.statusText === "Unauthorized") {
                        location.href = logOutUrl;
                    }
                }
            });
        }
        catch (err) {
            Shownotification(error, err.message + err.name)
        }
    }
    else { Shownotification(error, GetResourceValue('NoSettlementSelected')); return false; }
}
//----------------------------------------------------------- Debtor Offer line section end




//-----------------------------------------------------------Section Offer User Notes Start

function CreateDebOfferUserNotes() {
    try {
        $.ajax({
            type: "GET",
            contentType: 'application/json; charset=utf-8',
            url: "/DebtorAccount/CreateDebOfferUserNotes",
            success: function (data) {
                $("#popupEditDebOfferUserNotes").dxPopup({
                    title: $(data).find("#hdnTitle").val()

                }).dxPopup("instance").show();

                $('#EditDebOfferUserNotesForm').attr('action', $(data).find('#hdnActionName').val())
                $('#EditDebOfferUserNotesForm').html(data)
            },
            complete: function (response) {
                if (response.statusText === "Unauthorized" || response.statusText === "error") {
                    location.href = logOutUrl;
                }
            }
        });
    }
    catch (err) {
        Shownotification(error, err.message + err.name)
    }
}

function EditDeleteDebOfferUserNotes(rowData, e) {
    var strUrl;

    try {
        switch (e) {
            case 'DeleteDebOfferUserNotes':
                strUrl = "/DebtorAccount/DeleteDebOfferUserNotes";
                break;
            case 'EditDebOfferUserNotes':
                strUrl = "/DebtorAccount/EditDebOfferUserNotes";
                break;
            default:
                strUrl = "/DebtorAccount/EditDebOfferUserNotes"
        }
        $.ajax({
            type: "GET",
            traditional: true,
            contentType: 'application/json; charset=utf-8',
            url: strUrl,
            data: { cd: rowData.Created },
            success: function (data) {
                $("#popupEditDebOfferUserNotes").dxPopup({
                    title: $(data).find("#hdnTitle").val(),

                }).dxPopup("instance").show();
                $('#EditDebOfferUserNotesForm').attr('action', $(data).find('#hdnActionName').val())
                $('#EditDebOfferUserNotesForm').html(data)
            },
            complete: function (response) {
                if (response.statusText === "Unauthorized" || response.statusText === "error") {
                    location.href = logOutUrl;
                }
            }
        });
    }
    catch (err) {
        Shownotification(error, err.message + err.name)
    }

}

function CloseDebOfferUserNotesPopup() {
    $('#popupEditDebOfferUserNotes').dxPopup('instance').hide();
}

function OnDebOfferUserNotesSuccess(data) {
    if (data.success == true) {
        $('#popupEditDebOfferUserNotes').dxPopup('instance').hide();
        Shownotification("success", data.message)
        $('#dxGridDebOfferUserNotes').dxDataGrid('instance').refresh();
    }
    else if (data.success == false) {
        Shownotification("error", data.message)
    }
}

function OnDebOfferUserNotesFailure(data) {
    Shownotification("error", 'HTTP Status Code: ' + data.param1 + '  Error Message: ' + data.param2)
}

//-----------------------------------------------------------Section Offer User Notes End



//-----------------------------------------------------------Section Offer User Documents Start

function CreateDebOfferDocumentsInfo() {
    try {
        $.ajax({
            type: "GET",
            contentType: 'application/json; charset=utf-8',
            url: "/DebtorAccount/CreateDebOfferDocuments",
            success: function (data) {
                $("#popupEditDebOfferDocumentsInfo").dxPopup({
                    title: $(data).find("#hdnTitle").val()
                }).dxPopup("instance").show();

                $('#EditDebOfferDocumentsInfoForm').attr('action', $(data).find('#hdnActionName').val())
                $('#EditDebOfferDocumentsInfoForm').html(data)
            },
            complete: function (response) {
                if (response.statusText === "Unauthorized" || response.statusText === "error") {
                    location.href = logOutUrl;
                }
            }
        });
    }
    catch (err) {
        Shownotification(error, err.message + err.name)
    }
}

function EditDeleteDebOfferDocumentsInfo(rowData, e) {
    var strUrl;

    try {
        switch (e) {
            case 'DeleteDebOfferDocumentsInfo':
                strUrl = "/DebtorAccount/DeleteDebOfferDocuments";
                break;
            case 'EditDebOfferDocumentsInfo':
                strUrl = "/DebtorAccount/EditDebOfferDocuments";
                break;
            default:
                strUrl = "/DebtorAccount/EditDebOfferDocuments"

        }
        $.ajax({
            type: "GET",
            traditional: true,
            contentType: 'application/json; charset=utf-8',
            url: strUrl,
            data: { cd: rowData.Created },
            success: function (data) {
                $("#popupEditDebOfferDocumentsInfo").dxPopup({
                    title: $(data).find("#hdnTitle").val(),
                }).dxPopup("instance").show();

                $('#EditDebOfferDocumentsInfoForm').attr('action', $(data).find('#hdnActionName').val())
                $('#EditDebOfferDocumentsInfoForm').html(data)
            },
            complete: function (response) {
                if (response.statusText === "Unauthorized" || response.statusText === "error") {
                    location.href = logOutUrl;
                }
            }
        });
    }
    catch (err) {
        Shownotification(error, err.message + err.name)
    }
}

function CloseDebOfferDocumentsInfoPopup() {
    $('#popupEditDebOfferDocumentsInfo').dxPopup('instance').hide();
}

function OnDebOfferDocumentsInfoSuccess(data) {
    if (data.success == true) {
        $('#popupEditDebOfferDocumentsInfo').dxPopup('instance').hide();
        Shownotification("success", data.message)
        $('#dxGridDebOfferDocumentsInfo').dxDataGrid('instance').refresh();
    }
    else if (data.success == false) {
        Shownotification("error", data.message)
    }
}

function OnDebOfferDocumentsInfoFailure(data) {
    Shownotification("error", 'HTTP Status Code: ' + data.param1 + '  Error Message: ' + data.param2)
}

$(document).on("click", "#btnUploadDebOfferUserDocs,#btnDeleteDebOfferUserDocs", function (e) {
    e.preventDefault();
    var form = $('#EditDebOfferDocumentsInfoForm');
    var formData = new FormData($("#EditDebOfferDocumentsInfoForm").get(0));
    $.ajax({
        cache: false,
        async: false,
        type: "POST",
        url: form.attr('action'),
        data: formData,
        mimeType: "multipart/form-data",
        contentType: false,
        processData: false,
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            Shownotification("error", 'HTTP Status Code: ' + data.param1 + '  Error Message: ' + data.param2)
        },
        success: function (data) {
            var data = JSON.parse(data)
            if (data.success == true) {
                $('#popupEditDebOfferDocumentsInfo').dxPopup('instance').hide();
                Shownotification("success", data.message)
                $('#dxGridDebOfferDocumentsInfo').dxDataGrid('instance').refresh();
            }
            else if (data.success == false) {
                Shownotification("error", data.message)
            }
        }
    });
})

//-----------------------------------------------------------Section Offer User Documents End



//----------------------------------------------------------- Debtor Group section start

function CreateDebtorGroup() {
    try {
        $.ajax({
            type: "GET",
            async: false,
            contentType: 'application/json; charset=utf-8',
            url: "/DebtorAccount/CreateDepGroup",
            success: function (data) {
                $('#tabDebtorGroup .dx-tabs-wrapper').html('');
                $('div.modal-footer').remove();
                $("#popupEditDebtorGroup").dxPopup({ title: $(data).find("#hdnTitle").val() }).dxPopup("instance").show();
                var inst = $("#tabDebtorGroup").dxTabPanel({
                }).dxTabPanel("instance");
                var items = inst.option("items");
                items.length = 0
                inst.option("items", items);
                $(data).find('div.tabbable ul  a').each(function () {
                    var link = $(this).attr('href');
                    var text = $(this).text();
                    items.push({ title: text, template: "<br/><br/>" + $(this).parentsUntil('form').find('div' + link).html() + "<br/><br/>" });
                })
                inst.option("items", items);
                inst.option("deferRendering", false);
                inst.option("selectedIndex", 0);
                inst.option("animationEnabled", true);
                $(data).find('div.modal-footer').appendTo($("#tabDebtorGroup"));
                $('#EditDebtorGroupForm').attr('action', $(data).find('#hdnActionName').val())
            },
            complete: function (response) {
                if (response.statusText === "Unauthorized") {
                    location.href = logOutUrl;
                }
            }
        });
    }
    catch (err) {
        Shownotification(error, err.message + err.name)
    }
}

function ShowHidePopupDebtorGroup(rowData, e) {
    var strUrl;
    try {
        switch (e) {
            case 'ViewGroup':
                strUrl = "/DebtorAccount/DepGroupDetails";
                break;
            case 'EditGroup':
                strUrl = "/DebtorAccount/EditDepGroup";
                break;
            case 'DeleteGroup':
                strUrl = "/DebtorAccount/DeleteDepGroup";
                break;
            default:
                strUrl = "/DebtorAccount/DepGroupDetails"
        }
        $.ajax({
            type: "GET",
            traditional: true,
            contentType: 'application/json; charset=utf-8',
            url: strUrl,
            data: { id: rowData.RowId },
            success: function (data) {
                $('#tabDebtorGroup .dx-tabs-wrapper').html('');
                $('div.modal-footer').remove();
                $("#popupEditDebtorGroup").dxPopup({ title: $(data).find("#hdnTitle").val() }).dxPopup("instance").show();
                var inst = $("#tabDebtorGroup").dxTabPanel({
                }).dxTabPanel("instance");
                var items = inst.option("items");
                items.length = 0
                inst.option("items", items);
                $(data).find('div.tabbable ul  a').each(function () {
                    var link = $(this).attr('href');
                    var text = $(this).text();
                    items.push({ title: text, template: "<br/><br/>" + $(this).parentsUntil('form').find('div' + link).html() + "<br/><br/>" });
                })
                inst.option("items", items);
                inst.option("deferRendering", false);
                inst.option("selectedIndex", 0);
                inst.option("animationEnabled", true);
                $(data).find('div.modal-footer').appendTo($("#tabDebtorGroup"));
                $('#EditDebtorGroupForm').attr('action', $(data).find('#hdnActionName').val())
            },
            complete: function (response) {
                if (response.statusText === "Unauthorized") {
                    location.href = logOutUrl;
                }
            }
        });
    }
    catch (err) {
        Shownotification(error, err.message + err.name)
    }
}

function OnDebGroupSuccess(data) {

    if (data.success == true) {
        $('#popupEditDebtorGroup').dxPopup('instance').hide();
        Shownotification("success", data.message)
        $('#dxGridDebtorGroup').dxDataGrid('instance').refresh();
    }
    else if (data.success == false) {
        Shownotification("error", data.message)
    }
}

function OnDebGroupFailure(data) {
    Shownotification("error", 'HTTP Status Code: ' + data.param1 + '  Error Message: ' + data.param2)
}

function tabDebGrp_OncontReady(args) {
    window.setTimeout(function () {
        var validators = args.element.find(".dx-validator")
        validators.each(function (index, elemmen) {
            var elemment = $(elemmen).dxValidator("instance");
            elemment.on("validated", function (e) {
                if (!e.isValid) {
                    var inst = $("#tabDebtorGroup").dxTabPanel("instance");
                    var tabpanelItem = e.element.closest(".dx-multiview-item").data("dxMultiViewItemData");
                    inst.option("selectedItem", tabpanelItem);
                }
            });
        });

    }, 100);
}

//----------------------------------------------------------- Debtor Group section emd

//function setQtyForTotal(rowData, value) {
//    alert("1");
//    this.defaultSetCellValue(rowData, value);
//    qty = value;
//    var dataSourceConfiguration = {
//        store: DevExpress.data.AspNet.createStore({
//            key: "KeyStr",
//            loadUrl: '/DebtorAccount/RecalculateAmount',
//            loadParams: { 'qty': qty, 'price': price }
//        })
//    };
//    return dataSourceConfiguration;
//}
//function setPriceForTotal(rowData, value) {
//    alert("2");
//    this.defaultSetCellValue(rowData, value);
//    price = value;
//}

//function getTotal(rowData, value) {
//    $.getJSON('/DebtorAccount/RecalculateAmount', function (data, status) {
//        if (data != null) {
//            var a = data;
//        }
//    });
//}


/*************Debtor Update document************/

function ValidateVatAndCurrency(rowData) {
    var strUrl = "/DebtorAccount/ValidateVatAndCurrency";
    var grid = $('#dxGridDebtorOrders').dxDataGrid('instance');
    rowkey = grid.getSelectedRowKeys()[0];
    if (rowkey) {
        try {

            $.ajax({
                type: "GET",
                async: false,
                cache: false,
                contentType: 'application/json; charset=utf-8',
                url: strUrl,
                data: { rowId: rowkey },
                success: function (data) {

                },
                complete: function (response) {

                    if (response.statusText === "Unauthorized") {
                        location.href = logOutUrl;
                    }
                    else if (response.statusText === "OK") {
                        if (response.responseJSON.IsValid === false) {
                            //var LoadVoucherwindowConfirm = function () {
                            //    var result = DevExpress.ui.dialog.confirm(response.responseJSON.title, response.responseJSON.message);
                            //    result.done(function (dialogResult) {
                            //        dialogResult ? AllTransaction = true : AllTransaction = false;
                            //    }).then(function () {
                            //        LoadVoucherwindow();

                            //    });
                            //};


                            function beforeDisassociate() {
                                var returnValue = new $.Deferred();
                                var result = DevExpress.ui.dialog.confirm(response.responseJSON.message, response.responseJSON.title);
                                result.done(function (dialogResult) {
                                    returnValue.resolve(dialogResult);
                                });
                                return returnValue.promise();
                            }
                            beforeDisassociate().done(function (result) {
                                if (result === true) {
                                    DebtorUpdateDocument("Invoice");
                                }
                            });

                        }
                        else {
                            DebtorUpdateDocument("Invoice");
                        }
                    }
                }
            });
        }
        catch (err) {
            Shownotification(error, err.message + err.name)
        }
    }
    else { Shownotification(error, GetResourceValue('NoSettlementSelected')); return false; }
}



function DebtorUpdateDocument(rowData, rowId) {
    var strUrl = "/DebtorAccount/DebtorUpdateDocument";

    var grid = $('#dxGridDebtorOrders').dxDataGrid('instance');
    if (grid) {
        rowkey = grid.getSelectedRowKeys()[0];
    }
    else {
        rowkey = rowId;
    }

    if (rowkey) {
        try {

            $.ajax({
                type: "GET",
                async: false,
                cache: false,
                contentType: 'application/json; charset=utf-8',
                url: strUrl,
                data: { rowId: rowkey, DocType: rowData },
                success: function (data) {
                    var popup = $("#popupEditSalesOrderDoc").dxPopup({ title: $(data).find("#hdnTitle").val() }).dxPopup("instance");
                    var currentScreen = screenByWidth($(window).width());
                    if (currentScreen === 'lg') {
                        popup.option('height', 500)
                        popup.option('width', 400)
                    }
                    popup.content().empty(); $(data).appendTo(popup.content());
                    popup.show();
                    $("#dxFormDoc").dxForm('instance').repaint();
                    $('#EditSalesOrderFormDoc').attr('action', $(data).find('#hdnActionName').val()).attr('data-ajax-failure', 'OnDebOrdFailure(data)').attr('data-ajax-success', 'OnDebtorOrderSuccess')
                },
                complete: function (response) {


                    if (response.statusText === "Unauthorized") {
                        location.href = logOutUrl;
                    }
                }
            });
        }
        catch (err) {
            Shownotification(error, err.message + err.name)
        }
    }
    else { Shownotification(error, GetResourceValue('NoSettlementSelected')); return false; }


}



/*****************************/

//-----------------------------------------------------------Section DebAcc User Notes Start

function CreateDebUserNotes() {
    try {
        $.ajax({
            type: "GET",
            contentType: 'application/json; charset=utf-8',
            url: "/DebtorAccount/CreateDebUserNotes",
            success: function (data) {
                $("#popupEditDebUserNotes").dxPopup({
                    title: $(data).find("#hdnTitle").val()

                }).dxPopup("instance").show();

                $('#EditDebUserNotesForm').attr('action', $(data).find('#hdnActionName').val())
                $('#EditDebUserNotesForm').html(data)
            },
            complete: function (response) {
                if (response.statusText === "Unauthorized" || response.statusText === "error") {
                    location.href = logOutUrl;
                }
            }

        });

    }
    catch (err) {
        Shownotification(error, err.message + err.name)
    }
}

function EditDeleteDebUserNotes(rowData, e) {
    var strUrl;

    try {
        switch (e) {
            case 'DeleteDebUserNotes':
                strUrl = "/DebtorAccount/DeleteDebUserNotes";
                break;
            case 'EditDebUserNotes':
                strUrl = "/DebtorAccount/EditDebUserNotes";
                break;
            default:
                strUrl = "/DebtorAccount/EditDebUserNotes"

        }
        $.ajax({
            type: "GET",
            traditional: true,
            contentType: 'application/json; charset=utf-8',
            url: strUrl,
            //data: { id: rowData.TableRowId },
            data: { cd: rowData.Created },
            success: function (data) {
                $("#popupEditDebUserNotes").dxPopup({
                    title: $(data).find("#hdnTitle").val(),

                }).dxPopup("instance").show();
                $('#EditDebUserNotesForm').attr('action', $(data).find('#hdnActionName').val())
                $('#EditDebUserNotesForm').html(data)
            },
            complete: function (response) {
                if (response.statusText === "Unauthorized" || response.statusText === "error") {
                    location.href = logOutUrl;
                }
            }
        });
    }
    catch (err) {
        Shownotification(error, err.message + err.name)
    }

}

function CloseDebUserNotesPopup() {
    $('#popupEditDebUserNotes').dxPopup('instance').hide();
}

function OnDebUserNotesSuccess(data) {
    if (data.success == true) {
        $('#popupEditDebUserNotes').dxPopup('instance').hide();
        Shownotification("success", data.message)
        $('#dxGridDebUserNotes').dxDataGrid('instance').refresh();
    }
    else if (data.success == false) {
        Shownotification("error", data.message)
    }
}

function OnDebUserNotesFailure(data) {
    Shownotification("error", 'HTTP Status Code: ' + data.param1 + '  Error Message: ' + data.param2)
}

//-----------------------------------------------------------Section DebAcc User Notes End



//-----------------------------------------------------------Section DebAcc User Documents Start

function CreateDebDocumentsInfo() {
    try {
        $.ajax({
            type: "GET",
            contentType: 'application/json; charset=utf-8',
            url: "/DebtorAccount/CreateDebDocumentsInfo",
            success: function (data) {
                $("#popupEditDebDocumentsInfo").dxPopup({
                    title: $(data).find("#hdnTitle").val()
                }).dxPopup("instance").show();

                $('#EditDebDocumentsInfoForm').attr('action', $(data).find('#hdnActionName').val())
                $('#EditDebDocumentsInfoForm').html(data)
            },
            complete: function (response) {
                if (response.statusText === "Unauthorized" || response.statusText === "error") {
                    location.href = logOutUrl;
                }
            }
        });
    }
    catch (err) {
        Shownotification(error, err.message + err.name)
    }
}

function EditDeleteDebDocumentsInfo(rowData, e) {
    var strUrl;

    try {
        switch (e) {
            case 'DeleteDebDocumentsInfo':
                strUrl = "/DebtorAccount/DeleteDebDocumentsInfo";
                break;
            case 'EditDebDocumentsInfo':
                strUrl = "/DebtorAccount/EditDebDocumentsInfo";
                break;
            default:
                strUrl = "/DebtorAccount/EditDebDocumentsInfo"

        }
        $.ajax({
            type: "GET",
            traditional: true,
            contentType: 'application/json; charset=utf-8',
            url: strUrl,
            data: { cd: rowData.Created },
            success: function (data) {
                $("#popupEditDebDocumentsInfo").dxPopup({
                    title: $(data).find("#hdnTitle").val(),
                }).dxPopup("instance").show();

                $('#EditDebDocumentsInfoForm').attr('action', $(data).find('#hdnActionName').val())
                $('#EditDebDocumentsInfoForm').html(data)
            },
            complete: function (response) {
                if (response.statusText === "Unauthorized" || response.statusText === "error") {
                    location.href = logOutUrl;
                }
            }
        });
    }
    catch (err) {
        Shownotification(error, err.message + err.name)
    }
}

function CloseDebDocumentsInfoPopup() {
    $('#popupEditDebDocumentsInfo').dxPopup('instance').hide();
}

function OnDebDocumentsInfoSuccess(data) {
    if (data.success == true) {
        $('#popupEditDebDocumentsInfo').dxPopup('instance').hide();
        Shownotification("success", data.message)
        $('#dxGridDebDocumentsInfo').dxDataGrid('instance').refresh();
    }
    else if (data.success == false) {
        Shownotification("error", data.message)
    }
}

function OnDebDocumentsInfoFailure(data) {
    Shownotification("error", 'HTTP Status Code: ' + data.param1 + '  Error Message: ' + data.param2)
}

function setGrpKeyStr(data) {
    $('#GroupName').val($("#autoGrp").dxAutocomplete("instance").option().selectedItem.KeyName);
}

$(document).on("click", "#btnUploadDebUserDocs,#btnDeleteDebUserDocs", function (e) {
    e.preventDefault();
    var form = $('#EditDebDocumentsInfoForm');
    var formData = new FormData($("#EditDebDocumentsInfoForm").get(0));
    $.ajax({
        cache: false,
        async: false,
        type: "POST",
        url: form.attr('action'),
        data: formData,
        mimeType: "multipart/form-data",
        contentType: false,
        processData: false,
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            Shownotification("error", 'HTTP Status Code: ' + data.param1 + '  Error Message: ' + data.param2)
        },
        success: function (data) {
            var data = JSON.parse(data)
            if (data.success == true) {
                $('#popupEditDebDocumentsInfo').dxPopup('instance').hide();
                Shownotification("success", data.message)
                $('#dxGridDebDocumentsInfo').dxDataGrid('instance').refresh();
            }
            else if (data.success == false) {
                Shownotification("error", data.message)
            }
        }
    });
})

//-----------------------------------------------------------Section DebAcc User Documents End



//-----------------------------------------------------------Section DebOrd User Notes Start

function CreateDebOrdUserNotes() {
    try {
        $.ajax({
            type: "GET",
            contentType: 'application/json; charset=utf-8',
            url: "/DebtorAccount/CreateDebOrdUserNotes",
            success: function (data) {
                $("#popupEditDebOrdUserNotes").dxPopup({
                    title: $(data).find("#hdnTitle").val()

                }).dxPopup("instance").show();

                $('#EditDebOrdUserNotesForm').attr('action', $(data).find('#hdnActionName').val())
                $('#EditDebOrdUserNotesForm').html(data)
            },
            complete: function (response) {
                if (response.statusText === "Unauthorized" || response.statusText === "error") {
                    location.href = logOutUrl;
                }
            }

        });

    }
    catch (err) {
        Shownotification(error, err.message + err.name)
    }
}

function EditDeleteDebOrdUserNotes(rowData, e) {
    var strUrl;

    try {
        switch (e) {
            case 'DeleteDebOrdUserNotes':
                strUrl = "/DebtorAccount/DeleteDebOrdUserNotes";
                break;
            case 'EditDebUserNotes':
                strUrl = "/DebtorAccount/EditDebOrdUserNotes";
                break;
            default:
                strUrl = "/DebtorAccount/EditDebOrdUserNotes"

        }
        $.ajax({
            type: "GET",
            traditional: true,
            contentType: 'application/json; charset=utf-8',
            url: strUrl,
            //data: { id: rowData.TableRowId },
            data: { cd: rowData.Created },
            success: function (data) {
                $("#popupEditDebOrdUserNotes").dxPopup({
                    title: $(data).find("#hdnTitle").val(),

                }).dxPopup("instance").show();
                $('#EditDebOrdUserNotesForm').attr('action', $(data).find('#hdnActionName').val())
                $('#EditDebOrdUserNotesForm').html(data)
            },
            complete: function (response) {
                if (response.statusText === "Unauthorized" || response.statusText === "error") {
                    location.href = logOutUrl;
                }
            }
        });
    }
    catch (err) {
        Shownotification(error, err.message + err.name)
    }

}

function CloseDebOrdUserNotesPopup() {
    $('#popupEditDebOrdUserNotes').dxPopup('instance').hide();
}

function OnDebOrdUserNotesSuccess(data) {
    if (data.success == true) {
        $('#popupEditDebOrdUserNotes').dxPopup('instance').hide();
        Shownotification("success", data.message)
        $('#dxGridDebOrdUserNotes').dxDataGrid('instance').refresh();
    }
    else if (data.success == false) {
        Shownotification("error", data.message)
    }
}

function OnDebOrdUserNotesFailure(data) {
    Shownotification("error", 'HTTP Status Code: ' + data.param1 + '  Error Message: ' + data.param2)
}

//-----------------------------------------------------------Section DebOrd User Notes End



//-----------------------------------------------------------Section DebOrd User Documents Start

function CreateDebOrdDocumentsInfo() {
    try {
        $.ajax({
            type: "GET",
            contentType: 'application/json; charset=utf-8',
            url: "/DebtorAccount/CreateDebOrdDocumentsInfo",
            success: function (data) {
                $("#popupEditDebOrdDocumentsInfo").dxPopup({
                    title: $(data).find("#hdnTitle").val()
                }).dxPopup("instance").show();

                $('#EditDebOrdDocumentsInfoForm').attr('action', $(data).find('#hdnActionName').val())
                $('#EditDebOrdDocumentsInfoForm').html(data)
            },
            complete: function (response) {
                if (response.statusText === "Unauthorized" || response.statusText === "error") {
                    location.href = logOutUrl;
                }
            }
        });
    }
    catch (err) {
        Shownotification(error, err.message + err.name)
    }
}

function EditDeleteDebOrdDocumentsInfo(rowData, e) {
    var strUrl;

    try {
        switch (e) {
            case 'DeleteDebOrdDocumentsInfo':
                strUrl = "/DebtorAccount/DeleteDebOrdDocumentsInfo";
                break;
            case 'EditDebDocumentsInfo':
                strUrl = "/DebtorAccount/EditDebOrdDocumentsInfo";
                break;
            default:
                strUrl = "/DebtorAccount/EditDebOrdDocumentsInfo"

        }
        $.ajax({
            type: "GET",
            traditional: true,
            contentType: 'application/json; charset=utf-8',
            url: strUrl,
            data: { cd: rowData.Created },
            success: function (data) {
                $("#popupEditDebOrdDocumentsInfo").dxPopup({
                    title: $(data).find("#hdnTitle").val(),
                }).dxPopup("instance").show();

                $('#EditDebOrdDocumentsInfoForm').attr('action', $(data).find('#hdnActionName').val())
                $('#EditDebOrdDocumentsInfoForm').html(data)
            },
            complete: function (response) {
                if (response.statusText === "Unauthorized" || response.statusText === "error") {
                    location.href = logOutUrl;
                }
            }
        });
    }
    catch (err) {
        Shownotification(error, err.message + err.name)
    }
}

function CloseDebOrdDocumentsInfoPopup() {
    $('#popupEditDebOrdDocumentsInfo').dxPopup('instance').hide();
}

function OnDebOrdDocumentsInfoSuccess(data) {
    if (data.success == true) {
        $('#popupEditDebOrdDocumentsInfo').dxPopup('instance').hide();
        Shownotification("success", data.message)
        $('#dxGridDebOrdDocumentsInfo').dxDataGrid('instance').refresh();
    }
    else if (data.success == false) {
        Shownotification("error", data.message)
    }
}

function OnDebOrdDocumentsInfoFailure(data) {
    Shownotification("error", 'HTTP Status Code: ' + data.param1 + '  Error Message: ' + data.param2)
}

//function setGrpKeyStr(data) {
//    $('#GroupName').val($("#autoGrp").dxAutocomplete("instance").option().selectedItem.KeyName);
//}

$(document).on("click", "#btnUploadDebOrdUserDocs,#btnDeleteDebOrdUserDocs", function (e) {
    e.preventDefault();
    var form = $('#EditDebOrdDocumentsInfoForm');
    var formData = new FormData($("#EditDebOrdDocumentsInfoForm").get(0));
    $.ajax({
        cache: false,
        async: false,
        type: "POST",
        url: form.attr('action'),
        data: formData,
        mimeType: "multipart/form-data",
        contentType: false,
        processData: false,
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            Shownotification("error", 'HTTP Status Code: ' + data.param1 + '  Error Message: ' + data.param2)
        },
        success: function (data) {
            var data = JSON.parse(data)
            if (data.success == true) {
                $('#popupEditDebOrdDocumentsInfo').dxPopup('instance').hide();
                Shownotification("success", data.message)
                $('#dxGridDebOrdDocumentsInfo').dxDataGrid('instance').refresh();
            }
            else if (data.success == false) {
                Shownotification("error", data.message)
            }
        }
    });
})

//-----------------------------------------------------------Section DebOrd User Documents End