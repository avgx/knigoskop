jQuery(function ($) {
    $.datepicker.regional['ru'] = {
        closeText: 'Закрыть',
        prevText: '&#x3c;Пред',
        nextText: 'След&#x3e;',
        currentText: 'Сегодня',
        monthNames: ['Январь', 'Февраль', 'Март', 'Апрель', 'Май', 'Июнь',
        'Июль', 'Август', 'Сентябрь', 'Октябрь', 'Ноябрь', 'Декабрь'],
        monthNamesShort: ['Янв', 'Фев', 'Мар', 'Апр', 'Май', 'Июн',
        'Июл', 'Авг', 'Сен', 'Окт', 'Ноя', 'Дек'],
        dayNames: ['воскресенье', 'понедельник', 'вторник', 'среда', 'четверг', 'пятница', 'суббота'],
        dayNamesShort: ['вск', 'пнд', 'втр', 'срд', 'чтв', 'птн', 'сбт'],
        dayNamesMin: ['Вс', 'Пн', 'Вт', 'Ср', 'Чт', 'Пт', 'Сб'],
        weekHeader: 'Не',
        dateFormat: 'dd.mm.yy',
        firstDay: 1,
        isRTL: false,
        showMonthAfterYear: false,
        yearSuffix: ''
    };
    $.datepicker.setDefaults($.datepicker.regional['ru']);
});

jQuery.fn.ForceNumericOnly =
function () {
    return this.each(function () {
        $(this).keydown(function (e) {
            var key = e.charCode || e.keyCode || 0;
            // allow backspace, tab, delete, arrows, numbers and keypad numbers ONLY
            // home, end, period, and numpad decimal
            return (
                key == 8 ||
                key == 9 ||
                key == 46 ||
                key == 110 ||
                key == 190 ||
                (key >= 35 && key <= 40) ||
                (key >= 48 && key <= 57) ||
                (key >= 96 && key <= 105));
        });
    });
};
ko.bindingHandlers.autoComplete = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        var postUrl = allBindingsAccessor().source; // url to post to is read here
        var type = allBindingsAccessor().type;
        var selectedObservableArrayInViewModel = valueAccessor();
        $(element).autocomplete({
            minLength: 2,
            autoFocus: false,
            source: function (request, response) {
                $.ajax({
                    url: postUrl + '&searchtext=' + request.term,
                    dataType: "json",
                    type: "GET",
                    success: function (data) {
                        response(data.items);
                    }
                });
            },
            focus: function (event, ui) {
                $(element).val(ui.item.label);
                return false;
            },
            select: function (event, ui) {
                var selectedItem = ui.item;
                $(element).val("");
                if (type == 'serie') {
                    var serieModel = new IncomeSerieModel();
                    serieModel.Name(selectedItem.label);
                    serieModel.Id(selectedItem.value);

                    var result = $.grep(selectedObservableArrayInViewModel(), function (e) { return e.Id() == selectedItem.value; });
                    if (result.length == 0) {
                        selectedObservableArrayInViewModel.push(serieModel);
                    }
                }
                if (type == 'book') {
                    var bookModel = new IncomeSimilarBookModel();
                    bookModel.Name(selectedItem.label);
                    bookModel.Id(selectedItem.value);

                    var result = $.grep(selectedObservableArrayInViewModel(), function (e) { return e.Id() == selectedItem.value; });
                    if (result.length == 0) {
                        selectedObservableArrayInViewModel.push(bookModel);
                    }
                }
                else {
                    var authorModel = new IncomeAuthorModel();
                    authorModel.Name(selectedItem.label);
                    authorModel.Id(selectedItem.value);
                    authorModel.HasImage(selectedItem.hasImage);
                    var result = $.grep(selectedObservableArrayInViewModel(), function (e) { return e.Id() == selectedItem.value; });
                    if (result.length == 0) {
                        selectedObservableArrayInViewModel.push(authorModel);
                    }
                }

                // }
                return false;
            }
        })
        .data("ui-autocomplete")._renderItem = function (ul, item) {
            if (type == 'book') {
                //console.log(item);
                return $("<li>")
                  .append('<a>' + item.label + ', ' + item.desc + '</a>')
                  .appendTo(ul);
            } else if (type == 'serie') {
                return $("<li>")
                  .append("<a>" + item.label + "</a>")
                  .appendTo(ul);
            } else {
                var additionalText = '';
                if (typeof item.desc != 'undefined') {
                    if (item.desc != null && item.desc.length > 0) {
                        additionalText += ', ' + item.desc;
                    }
                }
                return $("<li>")
                  .append("<a>" + item.label + additionalText + "</a>")
                  .appendTo(ul);
            }
        };
        ;
    }
};

ko.bindingHandlers.datepicker = {
    init: function (element, valueAccessor, allBindingsAccessor) {

        var options = allBindingsAccessor().datepickerOptions || {},
            $el = $(element);

        $el.keypress(function (e) {
            var keycode = e.keyCode ? e.keyCode : e.which;
            if (keycode == 8 || keycode == 46) { // backspace
                // do somethiing
                var observable = valueAccessor();
                observable(null);
                $(this).val('')
            }
        });
        $el.datepicker({
            changeMonth: true,
            changeYear: true,
            yearRange: '1760:' + new Date().getFullYear().toString()
        });


        ko.utils.registerEventHandler(element, "change", function () {
            var observable = valueAccessor();
            observable($el.datepicker("getDate"));
        });


        ko.utils.domNodeDisposal.addDisposeCallback(element, function () {
            $el.datepicker("destroy");
        });

    },
    update: function (element, valueAccessor) {
        var value = ko.utils.unwrapObservable(valueAccessor()),
            $el = $(element);


        if (String(value).indexOf('/Date(') == 0) {
            value = new Date(parseInt(value.replace(/\/Date\((.*?)\)\//gi, "$1")));
        }

        var current = $el.datepicker("getDate");

        if (value - current !== 0) {
            $el.datepicker("setDate", value);
        }
    }
};

ko.bindingHandlers.imageUpload = {
    init: function (element, valueAccessor, allBindingsAccessor) {

        var value = ko.utils.unwrapObservable(valueAccessor());
        var uploadUrl = allBindingsAccessor().uploadUrl;
        new AjaxUpload($(element), {
            action: uploadUrl,
            name: 'image',
            autoSubmit: true,
            onSubmit: function (file, ext) {
                if (!(ext && /^(jpg|png|jpeg|gif)$/.test(ext))) {
                    alert("Некоректный формат изображения");
                    return false;
                }

            },
            onComplete: function (file, response) {
                var observable = valueAccessor();
                observable.ImageTempId(response);
                observable.HasImage(false);

            }
        });

    },
    update: function (element, valueAccessor) {

    }
};
ko.bindingHandlers.spinner = {
    init: function (element, valueAccessor, allBindingsAccessor) {

        var options = allBindingsAccessor().spinnerOptions || {};
        $(element).spinner(options);
        $(element).ForceNumericOnly();

        ko.utils.registerEventHandler(element, "spinchange", function () {
            var observable = valueAccessor();
            observable($(element).spinner("value"));
        });


        ko.utils.domNodeDisposal.addDisposeCallback(element, function () {
            $(element).spinner("destroy");
        });

    },
    update: function (element, valueAccessor) {
        var value = ko.utils.unwrapObservable(valueAccessor());

        current = $(element).spinner("value");
        if (value !== current) {
            $(element).spinner("value", value);
        }
    }
};

ko.bindingHandlers.dropdown = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        //console.log('init!');
        var a = allBindingsAccessor()['options'];
        var $element = $(element);

        var optionsText = allBindingsAccessor().optionsText;
        ko.applyBindingsToNode(element, { options: valueAccessor(), optionsText: optionsText }, viewModel);
        //$element.msDropdown();
    },
    update: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        //console.log('update!');
        var optionsText = allBindingsAccessor().optionsText;
        var $element = $(element);
        //$element.msDropdown();
    }
};

ko.bindingHandlers.ckEditor = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel) {
        var txtBoxID = $(element).attr("id");
        var options = allBindingsAccessor().richTextOptions || {};
        options.toolbar_Full = [
            ['Source', '-', 'Format', 'Font', 'FontSize', 'TextColor', 'BGColor', '-', 'Bold', 'Italic', 'Underline', 'SpellChecker'],
            ['NumberedList', 'BulletedList', '-', 'Outdent', 'Indent', '-', 'Blockquote', 'CreateDiv', '-', 'JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock', '-', 'BidiLtr', 'BidiRtl'],
            ['Link', 'Unlink', 'Image', 'Table']
        ];

        // handle disposal (if KO removes by the template binding)
        ko.utils.domNodeDisposal.addDisposeCallback(element, function () {
            if (CKEDITOR.instances[txtBoxID]) {
                CKEDITOR.remove(CKEDITOR.instances[txtBoxID]);
            }
        });

        //$(element).ckeditor(options);
        CKEDITOR.replace(element.id, {
            language: 'ru',
            uiColor: '#ffffff'
        });

        // wire up the blur event to ensure our observable is properly updated
        CKEDITOR.instances[txtBoxID].focusManager.blur = function () {
            CKEDITOR.instances[txtBoxID].updateElement();
            var observable = valueAccessor();
            observable($(element).val());
        };
    },
    update: function (element, valueAccessor, allBindingsAccessor, viewModel) {
        var val = ko.utils.unwrapObservable(valueAccessor());
        $(element).val(val);
    }
}

ko.extenders.required = function (target, overrideMessage) {

    target.hasError = ko.observable();
    target.validationMessage = ko.observable();
    target.IsValid = function () {
        return validate(target());

    }
    function validate(newValue) {
        var isValid = testRequired(newValue);
        target.hasError(!isValid);
        target.validationMessage(isValid == true ? "" : overrideMessage || "This field is required");
        return isValid;
    }
    function testRequired(val) {
        var stringTrimRegEx = /^\s+|\s+$/g,
       testVal;
        if (val === undefined || val === null) {
            return false;
        }

        testVal = val;
        if (typeof (val) === "string") {
            testVal = val.replace(stringTrimRegEx, '');
        }

        return ((testVal + '').length > 0);
    }

    target.subscribe(target.IsValid);
    return target;
};
ko.extenders.url = function (target, overrideMessage) {

    target.hasError = ko.observable();
    target.validationMessage = ko.observable();
    target.IsValid = function () {
        return validate(target());

    }
    function validate(newValue) {
        var isValid = testRequired(newValue);
        target.hasError(!isValid);
        target.validationMessage(isValid == true ? "" : overrideMessage || "Please enter a valid URL.");
        return isValid;
    }
    function testRequired(val) {
        return /^(https?|ftp):\/\/(((([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:)*@)?(((\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5]))|((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?)(:\d*)?)(\/((([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)+(\/(([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)*)*)?)?(\?((([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)|[\uE000-\uF8FF]|\/|\?)*)?(\#((([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)|\/|\?)*)?$/i.test(val);
    }

    target.subscribe(target.IsValid);
    return target;
};
ko.extenders.email = function (target, overrideMessage) {

    target.hasError = ko.observable();
    target.validationMessage = ko.observable();
    target.IsValid = function () {
        return validate(target());

    }
    function validate(newValue) {
        var isValid = testRequired(newValue);
        target.hasError(!isValid);
        target.validationMessage(isValid == true ? "" : overrideMessage || "Please enter a valid Email.");
        return isValid;
    }
    function testRequired(val) {
        return /^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))$/i.test(val);
    }

    target.subscribe(target.IsValid);
    return target;
};
//ko.extenders.ajaxValidate = function (target, data) {

//    target.hasError = ko.observable();
//    target.validationMessage = ko.observable();
//    target.IsValid = function () {
//        return validate(target());

//    }
//    function validate(newValue) {
//        var isValid = testRequired(newValue);
//        target.hasError(!isValid);
//        target.validationMessage(isValid == true ? "" : data.overrideMessage || "Please enter a valid OPDS url.");
//        return isValid;
//    }
//    function testRequired(val) {
//        var result = null;
//        $.post(data.url + val, function (data) {
//            result  = data.success;
//        });

//        return result;
//        //$.ajax({
//        //    url: data.url + val,                
//        //    type: "POST",
//        //    async: false,
//        //    success: function (data) {
//        //        return true;
//        //    }
//        //});

//    }

//    target.subscribe(target.IsValid);
//    return target;
//};
ko.extenders.arrayRequired = function (target, overrideMessage) {

    target.hasError = ko.observable();
    target.validationMessage = ko.observable();
    target.IsValid = function () {
        return validate(target());

    }
    function validate(newValue) {
        var isValid = testRequired(newValue);
        target.hasError(!isValid);
        target.validationMessage(isValid == true ? "" : overrideMessage || "This field is required");
        return isValid;
    }
    function testRequired(val) {
        return val.length > 0;
    }

    target.subscribe(target.IsValid);
    return target;
};
function InitValidation(target) {

    target.IsValid = function () {
        var result = true;
        for (var prop in target) {
            if (ko.isObservable(target[prop])) {
                if (target[prop].hasOwnProperty('hasError')) {
                    result &= target[prop].IsValid()

                }
            }
        }
        return result;
    }
}