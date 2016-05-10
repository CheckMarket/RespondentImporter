#Respondent Importer

Using [our API](https://api.checkmarket.com) it is possible to import respondents for a certain survey. To demonstrate this feature, we have built this custom example application showing you how you can achive the import.

In this demo, you will be able choose a LIVE survey from your account. Then you can upload a csv file with the results you would like to import. 



##Prerequisites
1. A free CheckMarket account. [Sign up](https://www.checkmarket.com/sign-up/) if you don't have one yet.
2. API keys, you can [create them](https://api-eu.checkmarket.com/Account/Keys) in your CheckMarket account. Your key needs the role *Results (Write)* and should be stored in your UserSecrets.
3. Environment in which you are able to run this ASP.NET Core 1 application

##API requests
In this demo we are using the API to retrieve all required information to activate the import: 

>[GET - /3/surveys](https://api.checkmarket.com/docs/api/v3/action/GET-3-Surveys_lang_top_skip_select_filter_orderby_expand)
Retrieve the surveyId, Title and Languages for all live surveys


>[GET - /3/surveys/{surveyid}/questions](https://api.checkmarket.com/docs/api/v3/action/GET-3-surveys-SurveyId-questions_asFlatList_stripHtml_lang_top_skip_select_filter)
Retrieve the questions for the selected survey

>[POST - 3/surveys/{surveyid}/respondents](https://api.checkmarket.com/docs/api/v3/action/POST-3-surveys-SurveyId-respondents_IncludeSuccessResponses)
Send the new respondents
