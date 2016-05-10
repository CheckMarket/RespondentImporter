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


