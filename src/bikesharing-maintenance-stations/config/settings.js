'use strict';

var config = {
	env: process.env.NODE_ENV || 'development',
	root: process.cwd(),
	port: process.env.PORT || 3123,
	navLinks: {
		dashboard: process.env.APPSETTING_DASHBOARD_LINK || "http://localhost:1935/",
		issues: process.env.APPSETTING_ISSUES_LINK || "http://localhost:2915/Issue",
		subscription: process.env.APPSETTING_SUBSCRIPTION_LINK || "http://localhost:2915/Subscription",
		rides: process.env.APPSETTING_RIDES_LINK || "http://localhost:2915",
	},
	stationsApiBaseUrl: process.env.APPSETTING_STATIONSAPI_BASEURL || "http://bikesharing360-rides-dev.azurewebsites.net/",
	accountsBaseUrl: process.env.APPSETTING_ACCOUNTS_BASEURL || "http://localhost:2915/"
};

module.exports = config;