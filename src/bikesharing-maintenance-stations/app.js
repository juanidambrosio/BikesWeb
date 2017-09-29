require('dotenv').config();
var express = require('express');
var app = express();

var settings = require('./config/settings');
require('./config/express')(app);

var server = app.listen(settings.port, function () {
    console.log("App listening at port %s", 
    this.address().port);
});

module.exports = app;
