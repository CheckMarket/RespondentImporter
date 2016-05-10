var CM = CM || {};
CM.apps = CM.apps || {}

CM.apps.apifactory = function () {
    function getApiRequest(query) {
        var deferred = $.Deferred();
        $.getJSON(query)
            .done(function (result) {
                deferred.resolve(result);
            })
            .fail(function (d, textStatus, error) {
                console.error("Api request to '" + query + "' failed")
                deferred.reject(d, textStatus, error);
            });

        return deferred.promise();
    }
    return {
        GetSurveys: function () {
            return getApiRequest('/api/surveys');
        },
        GetQuestions: function (SurveyId) {
            return getApiRequest('/api/surveys/' + SurveyId + '/questions');
        },
        GetQuestion: function (SurveyId, QuestionId) {
            return getApiRequest('/api/surveys/' + SurveyId + '/questions/' + QuestionId);
        }
    }
}();



/// <reference path='./ApiFactory.js'/>
var CM = CM || {};
CM.apps = CM.apps || {}

CM.apps.importer = function () {
    var isValid = false;
    function SetValidationState(element, isvalid) {
        if (isvalid) {
            $(element).parent('.form-group').removeClass('has-error');
            $(element).next('.help-block').remove();
        } else {
            isValid = false;
            $(element).parent('.form-group').addClass('has-error');
            if (! $(element).next('.help-block').length)
                $(element).parent('.form-group').append("<span class='help-block'>Required</span>");
        }
    }
    function ValidateAndSubmit() {
        isValid = true;
        $('#frmReadFile #ddlSurveys').trigger('validate');
        $('#frmReadFile #uploadFile').trigger('validate');

        if (isValid) {
            $('#frmReadFile').submit();
        } 
    }
    function LoadSurveys(){
        CM.apps.apifactory.GetSurveys().done(function (result) {
            $.each(result.Data, function (index, survey) {
                $('#ddlSurveys').append(
                    ["<option value=", survey.Id," langs=", survey.Langs.join('') ,">", survey.Title ,"</option>"].join('')
                );
            });
        });
    
    }
    return {
        Initialize: function () {
            LoadSurveys();

            $('#frmReadFile #ddlSurveys').on('change validate', function () {
                SetValidationState(this, ($(this).val() != "0"));
            });
            $('#frmReadFile #uploadFile').on('change validate', function () {
                SetValidationState(this, ($(this).val() != ""));
            });
            $('#btnSubmit').on('click', function (e) {
                ValidateAndSubmit();
            });
        }
    }
}();
CM.apps.configurator = function () {
    return {
        Initialize: function () {
            CM.apps.apifactory.GetQuestions(View.SurveyId).done(function (questions) {
                var optQuestions = "<option value=0>Please select</option>";
                $.each(questions.Data, function (index, q) {
                    if (q.SubQuestions != undefined) {
                        $.each(q.SubQuestions, function (subindex, qchild) {
                            optQuestions += ["<option value=", qchild.Id, ">", qchild.Caption, "</option>"].join("");
                        });
                    } else {
                        optQuestions += ["<option value=", q.Id, ">", q.Caption, "</option>"].join("");
                    }
                });
                $("select.question").html(optQuestions);
            });
            $("select.question").on('change', function () {
                var questionId = $(this).val();
                var ddlResponses = $(this).next('.responses');
                $(ddlResponses).children('option[value!=0]').remove();
                if (questionId != "0") {
                    CM.apps.apifactory.GetQuestion(View.SurveyId, questionId).done(function (question) {
                        var optResponses = "<option value=0>Rows contain response</option>";
                        if (question.Data.QuestionResponses.length > 1) {
                            $.each(question.Data.QuestionResponses, function (index, response) {
                                optResponses += ["<option value='", response.ResponseId, "'>", response.Caption, "</option>"].join('');
                            });
                        }
                        $(ddlResponses).html(optResponses).removeClass("hidden");

                    });
                } else {
                    $(ddlResponses).addClass("hidden");
                }
            });
        }
    }
}();