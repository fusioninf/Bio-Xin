const loaderSpinnerSelector = '#loader-spinner'

var Common = function () {
    // binding events as soon as the object is instantiated
    this.bindEvents();
};

Common.prototype = {

    bindEvents: function () {
        jQuery('.datepicker').datepicker({
            autoclose: true,
            todayHighlight: true,
            format: 'dd-M-yyyy'
        });
    },
    ajaxCallPostRequest: (url, payload, callback = null, async = true, dataType = 'json') => {
        $.ajax({
            url: url,
            type: "POST",
            dataType: "json",
            data: payload,
            async: async,
            beforeSend: function () {
                $(loaderSpinnerSelector).show();
            },
            success: function (response) {
                debugger;
                if (response.unAuthorized !== undefined && response.unAuthorized === 1)
                    window.location.href = response.redirectTo;

                if (callback)
                    callback(response);
            },
            error: (response) => {
                alert('Something went wrong, Please try again');
            },
            complete: function () {
                $(loaderSpinnerSelector).hide();
            }
        });
    },
    ajaxCallGetRequest: (url, callback = null, async = true, dataType = 'json') => {
        $.ajax({
            url: url,
            type: "get",
            dataType: dataType,
            async: async,
            beforeSend: function () {
                $(loaderSpinnerSelector).show();
            },
            success: function (response) {

                if (response.unAuthorized !== undefined && response.unAuthorized === 1)
                    window.location.href = response.redirectTo;

                if (callback)
                    callback(response);
            },
            error: (response) => {
                console.log(response);
                alert(response);
            },
            complete: function () {
                $(loaderSpinnerSelector).hide();
            }
        });
    },
    bindDropdown: (selector, values, valueProp, TextProp, selectedValue, defaultLabel = '--Select--') => {

        $(selector).empty();
        if (defaultLabel !== '')
            $(selector).append(`<option value="">${defaultLabel}</option>`);

        values.map((item) => {
            $(selector).append(`<option ${selectedValue == item[valueProp] ? 'selected' : ''} value="${item[valueProp]}">${item[TextProp]}</option>`);
        })
    }
}