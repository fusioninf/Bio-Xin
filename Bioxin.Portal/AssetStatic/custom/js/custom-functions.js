//$(document).ready(function() {


//});
function OpenPopUp(pageName) {
    ; 
    //window.open("~/ReportViewerForm/" + pageName, "_blank", "WIDTH=1024,HEIGHT=768,scrollbars=no, menubar=no,resizable=yes,directories=no,location=no");
    window.open("./ReportViewerForm/" + pageName, "_blank", "WIDTH=1024,HEIGHT=768,scrollbars=no, menubar=no,resizable=yes,directories=no,location=no");
    //window.open("~/ReportViewerForm/BasicReportViewer.aspx", "_blank", "WIDTH=1080,HEIGHT=790,scrollbars=no, menubar=no,resizable=yes,directories=no,location=no");
}

function LoadDate(controlName) {
    jQuery(controlName).datepicker({
        autoclose: true,
        todayHighlight: true,
        format: 'dd-M-yyyy'
      /*  format: 'dd/mm/yyyyy'*/
    });
}




function LoadDate_C(controlName) {
    jQuery(controlName).datepicker({
        autoclose: true,
        todayHighlight: true,
        format: 'dd-M-yyyy'
        
    }).datepicker("setDate", new Date());
}



function LoadDate_P(controlName) {
    jQuery(controlName).datepicker({
        autoclose: true,
        todayHighlight: true,
        format: 'dd-M-yyyy'
        
    }).datepicker("setDate", "-1d");
}


//$( ".selector" ).datepicker( "setDate", 15);

function ToJavaScriptDate(value) {
   
    var pattern = /Date\(([^)]+)\)/;
    var results = pattern.exec(value);
    var dt = new Date(parseFloat(results[1]));
    var dayData = dt.getDate();
    if (dayData < 10) {
        dayData = "0" + dayData;
    }
    var monthNames = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
    var rDate = dayData + "-" + monthNames[(dt.getMonth())] + "-" + dt.getFullYear();
    return rDate;
}

function ValidationColorChange(controlName, divName, isDropdown) {

    var flag = 'input';
    if (isDropdown == true) {
        flag = 'change';
    }

    $('#' + divName + '').addClass("has-warning"); 
    //$('#' + controlName + '').addClass("form-control-warning"); 
    $('#' + controlName + '').addClass("is-warning");
    
    $('#' + controlName + '').bind(flag, function () {
        $('#' + divName + '').removeClass("has-warning");
        //$('#' + controlName + '').removeClass("form-control-warning");

        $('#' + controlName + '').removeClass("is-warning");
    });
}

function ShowMessage(msgText) {
    swal(msgText);
}

function AddDays(date, days) {
    var result = new Date(date);
    result.setDate(result.getDate() + days);
    return result;
}

function validateEmail(mail) {
    var pattern = /^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$/;

    return $.trim(mail).match(pattern) ? true : false;
}
function DateFormatChange(dateParam) {
    var todaydate = new Date(dateParam);  //pass val varible in Date(val)
    var dd = todaydate.getDate();
    //var mm = todaydate.getMonth() + 1; //January is 0!
    var mm = todaydate.getMonth();
    var yyyy = todaydate.getFullYear();
    if (dd < 10) { dd = '0' + dd }
    //if (mm < 10) { mm = '0' + mm }
    var monthNames = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
    var date = dd + '-' + monthNames[mm] + '-' + yyyy;
    return date;
}
function ValidationColorChangeNew(controlName, divName, message, isDropdown) {
    
    $('#' + divName + ' ' + '.customLabel').empty();
    var flag = 'input';
    if (isDropdown == true) {
        flag = 'change';
    }

    $('#' + divName + '').append('<div class="customLabel"><p class="text-danger">' + message + '</p></div>');

    $('#' + divName + '').addClass("has-warning");

    //$('#' + controlName + '').addClass("form-control-warning");

    $('#' + controlName + '').bind(flag, function () {
        $('#' + divName + ' ' + '.customLabel').empty();
        $('#' + divName + '').removeClass("has-warning");
        $('#' + controlName + '').removeClass("form-control-warning");
    });
}

function LoadPaymentType(elementId)
{
    $.ajax({
        url: "/Utility/GetPaymentType",
        contentType: "application/json; charset=utf-8",
        dataType: 'json',
        type: "Get",

        async: false,
        success: function (data)
        {

            $('#' + elementId + '').empty();
            $('#' + elementId + '').append("<option value='0'>--Select Payment --</option>");

            for (var i = 0; i < data.length; i++) {

                $('#' + elementId + '').append($("<option></option>").val(data[i].PaymentTypeId).html(data[i].PaymentType));
            }
        }
    });
}


function LoadCollectionType(elementId)
{
    $.ajax({
        url: "/Utility/GetPaymentType",
        contentType: "application/json; charset=utf-8",
        dataType: 'json',
        type: "Get",

        async: false,
        success: function (data)
        {

            $('#' + elementId + '').empty();
            $('#' + elementId + '').append("<option value='0'>--Select Collection Type --</option>");

            for (var i = 0; i < data.length; i++) {

                $('#' + elementId + '').append($("<option></option>").val(data[i].PaymentTypeId).html(data[i].PaymentType));
            }
        }
    });
}


function LoadBankName(elementId)
{
    $.ajax({
        url: "/Utility/GetBankName",
        contentType: "application/json; charset=utf-8",
        dataType: 'json',
        type: "Get",

        async: false,
        success: function (data)
        {

            $('#' + elementId + '').empty();
            $('#' + elementId + '').append("<option value='0'>--Select Bank --</option>");

            for (var i = 0; i < data.length; i++) {

                $('#' + elementId + '').append($("<option></option>").val(data[i].BankId).html(data[i].BankName));
            }
        }
    });
}

function LoadBankBranch(elementId, bankid)
{
    $.ajax({
        url: "/Utility/GetBankBranch",
        contentType: "application/json; charset=utf-8",
        dataType: 'json',
        type: "Get",
        data: { bankid: bankid },
        async: false,
        success: function (data)
        {

            $('#' + elementId + '').empty();
            $('#' + elementId + '').append("<option value='0'>--Select Branch --</option>");

            for (var i = 0; i < data.length; i++) {

                $('#' + elementId + '').append($("<option></option>").val(data[i].BranchId).html(data[i].BranchName));
            }
        }
    });
}
function LoadReasonData(elementId, reasonFor) {

    debugger;
    $.ajax({
        url: "/Utility/GetReason",
        contentType: "application/json; charset=utf-8",
        dataType: 'json',
        type: "Get",
        data: { reasonFor: reasonFor },
        async: false,
        success: function (data)
        {

            $('#' + elementId + '').empty();
            $('#' + elementId + '').append("<option value='0'>--Select Reason --</option>");

            for (var i = 0; i < data.length; i++) {

                $('#' + elementId + '').append($("<option></option>").val(data[i].ReasonId).html(data[i].Name));
            }
        }
    });
}


function LoadCompany(elementId) {
  
    
    $.ajax({
        url: "/Utility/LoadCompany",
        contentType: "application/json; charset=utf-8",
        dataType: 'json',
        type: "Get",
        data: {},
        async: false,
        success: function (data) {
          
            $('#' + elementId + '').empty();
            $('#' + elementId + '').append("<option value='0'>--Select Company--</option>");

            for (var i = 0; i < data.length; i++) {

                $('#' + elementId + '').append($("<option></option>").val(data[i].CompanyId).html(data[i].CompanyName));
            }
        }
    });
}


function EmpAutoCom(autoElement, noElement) {
   

    var UserID = $('#sessionUserInfo').val();
    $('#' + autoElement + '').val(UserID);
    $('#' + autoElement + '').autocomplete({

        source: "/Utility/GetEmpAutoComAll", 
        select: function (event, ui) {
            // console.log(ui);
     
            var a = ui.item.value;
            $('#' + autoElement + '').val(ui.item.value);
            
            var theUser =  $('#' + autoElement + '').val();
            GetEmpNo(theUser, noElement);
            //GetEmployeeDetails();
            //ShowSalaryDetails();

        }

    });

    if (UserID != '') {
        GetEmpNo(UserID, noElement);
    }
    $('#' + autoElement + '').autocomplete("widget").addClass("fixedHeightAuto");
    $('#' + autoElement + '').autocomplete("widget").addClass("overLap");

}

function GetEmpNo(data, element) {

    var a = data;
    var empId = data.substring(0, 5);
    var urlpath = "/Utility/GetEmpNo";
    $.ajax({
        url: urlpath,
        dataType: 'json',
        type: "Get",
        data: { empId: empId },
        async: false,
        success: function (data) {

            if (data) {
                $('#' + element + '').val(data);

            } else {
                $('#' + element + '').val(0);
            }
        }
    });
};

function LoadLocation(elementId, comid=0){
        var urlpath = "/Utility/LoadLocation"; 
        $.ajax({
            url: urlpath,
            type: "Get",
            data: { comid: comid },
            async: false,
            success: function (data)
            {

                $('#' + elementId + '').empty();
                $('#' + elementId + '').append("<option value='0'>--Select Location--</option>");
                for (var i = 0; i < data.length; i++) {

                    $('#' + elementId + '').append($("<option></option>").val(data[i].LocationId).html(data[i].LocationName));
                }

            }
        });
    }

    function LoadVehicle(elementId)
    {
        var urlpath = "/Utility/LoadVehicle"; 
        $.ajax({
            url: urlpath,
            type: "Get",
            data: {},
            async: false,
            success: function (data)
            {

                $('#' + elementId + '').empty();
                $('#' + elementId + '').append("<option value='0'>--Select Vehicle No--</option>");
                for (var i = 0; i < data.length; i++) {

                    $('#' + elementId + '').append($("<option></option>").val(data[i].VehicleInfoId).html(data[i].VehicleRegNo));
                }

            }
        });
    }

function LoadWareHouse(elementId, locid = 0, wareHouseTypeid = 0) {

    debugger;
    
        var urlpath = "/Utility/LoadWareHouse";
        $.ajax({
            url: urlpath,
            type: "Get",
            data: { locid: locid, wareHouseTypeid: wareHouseTypeid },
            async: false,
            success: function (data)
            {

                $('#' + elementId + '').empty();
                $('#' + elementId + '').append("<option value='0'>--Select Warehouse--</option>");
                for (var i = 0; i < data.length; i++) {

                    $('#' + elementId + '').append($("<option></option>").val(data[i].WareHouseId).html(data[i].WareHouseName));
                }

            }
        });
    }

        function LoadWareHouseMultipleType(elementId, locid, wareHouseTypeid) {
      
            var urlpath = "/Utility/LoadWareHouseMultipleType";
            $.ajax({
                url: urlpath,
                type: "Get",
                data: { locid: locid, wareHouseTypeid: wareHouseTypeid },
                async: false,
                success: function (data)
                {

                    $('#' + elementId + '').empty();
                    $('#' + elementId + '').append("<option value='0'>--Select Warehouse--</option>");
                    for (var i = 0; i < data.length; i++) {

                        $('#' + elementId + '').append($("<option></option>").val(data[i].WareHouseId).html(data[i].WareHouseName));
                    }

                }
            });
        }


        function LoadProduct(elementId)
        {
            var urlpath = "/PortionStockIn/LoadProduct"; 
            $.ajax({
                url: urlpath,
                type: "Get",
                data: {},
                async: false,
                success: function (data)
                {

                    $('#' + elementId + '').empty();
                    $('#' + elementId + '').append("<option value='0'>--Select Product--</option>");
                    for (var i = 0; i < data.length; i++) {

                        $('#' + elementId + '').append($("<option></option>").val(data[i].ProductId).html(data[i].ProductName));
                    }

                }
            });
        }
        function LoadAllProduct(elementId)
        {
            var urlpath = "/PortionStockIn/LoadAllProduct"; 
            $.ajax({
                url: urlpath,
                type: "Get",
                data: {},
                async: false,
                success: function (data)
                {

                    $('#' + elementId + '').empty();
                    $('#' + elementId + '').append("<option value='0'>--Select Product--</option>");
                    for (var i = 0; i < data.length; i++) {

                        $('#' + elementId + '').append($("<option></option>").val(data[i].ProductId).html(data[i].ProductName));
                    }

                }
            });
        }

        function LoadDivision(elementId)
        {
            var urlpath = "/Utility/GetDivision";
            $.ajax({
                url: urlpath,
                contentType: "application/json; charset=utf-8",
                dataType: 'json',
                type: "Get",
                data: {},
                async: false,
                success: function (data)
                {

                    $('#' + elementId + '').empty();
                    $('#' + elementId + '').append("<option value='0'>--Select Division--</option>");

                    for (var i = 0; i < data.length; i++) {

                        $('#' + elementId + '').append($("<option></option>").val(data[i].DivisionId).html(data[i].DivisionName));
                    }
                }
            });
        }



        function LoadAreaByDivisionId(elementId,divisionId=0)
            {

   
                var urlpath = "/Utility/GetArea";

                $.ajax({
                    url: urlpath,
                    contentType: "application/json; charset=utf-8",
                    dataType: 'json',
                    type: "Get",
                    data: { division: divisionId },
                    async: false,
                    success: function (data)
                    {

                        $('#' + elementId + '').empty();
                        $('#' + elementId + '').append("<option value='0'>--Select Area--</option>");

                        for (var i = 0; i < data.length; i++) {

                            $('#' + elementId + '').append($("<option></option>").val(data[i].AreaId).html(data[i].AreaName));
                        }
                    }
                });
            }

            function LoadDistrictByDivisionId(elementId,divisionId=0)
                {
                    var urlpath = "/Utility/GetDistrict";
                    $.ajax({
                        url: urlpath,
                        contentType: "application/json; charset=utf-8",
                        dataType: 'json',
                        type: "Get",
                        data: { division: divisionId },
                        async: false,
                        success: function (data)
                        {

                            $('#' + elementId + '').empty();
                            $('#' + elementId + '').append("<option value='0'>--Select District--</option>");

                            for (var i = 0; i < data.length; i++) {

                                $('#' + elementId + '').append($("<option></option>").val(data[i].DistrictId).html(data[i].DistrictName));
                            }
                        }
                    });
                }
                function LoadThanaByDistrictId(elementId,districtId=0)
                    {
                        var urlpath = "/Utility/GetThana";
                        $.ajax({
                            url: urlpath,
                            contentType: "application/json; charset=utf-8",
                            dataType: 'json',
                            type: "Get",
                            data: { district: districtId },
                            async: false,
                            success: function (data)
                            {

                                $('#' + elementId + '').empty();
                                $('#' + elementId + '').append("<option value='0'>--Select Thana--</option>");

                                for (var i = 0; i < data.length; i++) {

                                    $('#' + elementId + '').append($("<option></option>").val(data[i].ThanaId).html(data[i].ThanaName));
                                }
                            }
                        });
                    }

                    function LoadTerritoryByAreaId(elementId,areaId=0)
                        {
                            var urlpath = "/Utility/GetTerritory";
                            $.ajax({
                                url: urlpath,
                                contentType: "application/json; charset=utf-8",
                                dataType: 'json',
                                type: "Get",
                                data: { area: areaId },
                                async: false,
                                success: function (data)
                                {

                                    $('#' + elementId + '').empty();
                                    $('#' + elementId + '').append("<option value='0'>--Select Territory--</option>");

                                    for (var i = 0; i < data.length; i++) {

                                        $('#' + elementId + '').append($("<option></option>").val(data[i].TerritoryId).html(data[i].TerritoryName));
                                    }
                                }
                            });
                        }


                        function LoadMarketByTerritoryId(elementId,territoryId=0)
                            {

                                var urlpath = "/Utility/GetMarket";
                                $.ajax({
                                    url: urlpath,
                                    contentType: "application/json; charset=utf-8",
                                    dataType: 'json',
                                    type: "Get",
                                    data: { territory: territoryId },
                                    async: false,
                                    success: function (data)
                                    {

                                        $('#' + elementId + '').empty();
                                        $('#' + elementId + '').append("<option value='0'>--Select Territory--</option>");

                                        for (var i = 0; i < data.length; i++) {

                                            $('#' + elementId + '').append($("<option></option>").val(data[i].MarketId).html(data[i].MarketName));
                                        }
                                    }
                                });
                            }

                            function LoadFilteredCustomers(elementId,divisionId=0,areaId=0, territoryId=0,customerTypeId=0) {
     
                                $('#' + elementId + '').val('');
                                var urlpath = "/Customer/GetFilteredCustomerList";
                                $.ajax({
                                    url: urlpath,
                                    contentType: "application/json; charset=utf-8",
                                    dataType: 'json',
                                    type: "Get",
                                    data: { divisionId: divisionId, areaId: areaId, territoryId: territoryId, customerTypeId: customerTypeId },
                                    async: false,
                                    success: function (data)
                                    {

                                        $('#' + elementId + '').empty();
                                        $('#' + elementId + '').append("<option value='0'>--Select Customer--</option>");

                                        for (var i = 0; i < data.length; i++) {
                 
                                            $('#' + elementId + '').append($("<option></option>").val(data[i].CustomerID).html(data[i].CustomerName + ":" + data[i].CustomerCode + ":" + data[i].CustomerTypeName));
                                        }
                                    }
                                });
                            }

                                function LoadCustomerType(elementId) {

    
                                    var urlpath = "/SalesOrderFurtherProcess/GetCustomerType";
                                    $.ajax({
                                        url: urlpath,
                                        contentType: "application/json; charset=utf-8",
                                        dataType: 'json',
                                        type: "Get",

                                        async: false,
                                        success: function (data) {

                                            $('#' + elementId + '').empty();
                                            $('#' + elementId + '').append("<option value='0'>--Select Customer Type --</option>");

                                            for (var i = 0; i < data.length; i++) {

                                                $('#' + elementId + '').append($("<option></option>").val(data[i].CustomerTypeID).html(data[i].CoustomerTypeName));
                                            }
                                        }
                                    });
                                }
