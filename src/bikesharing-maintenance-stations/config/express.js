var express = require("express");
var path = require("path");
var logger = require("morgan");
var exphbs  = require("express-handlebars");
var glob = require("glob");
var settings = require("./settings");

module.exports = function (app) {
    var hbs = exphbs.create({
        defaultLayout: "main",
        helpers: {
            eq: function (v1, v2) {
                return v1 == v2;
            },
            ne: function (v1, v2) {
                return v1 != v2;
            },
            lt: function (v1, v2) {
                return v1 < v2;
            },
            gt: function (v1, v2) {
                return v1 > v2;
            },
            lte: function (v1, v2) {
                return v1 <= v2;
            },
            gte: function (v1, v2) {
                return v1 >= v2;
            },
            and: function (v1, v2) {
                return v1 && v2;
            },
            or: function (v1, v2) {
                return v1 || v2;
            }
        }
    });
    app.engine("handlebars", hbs.engine);
    app.set("view engine", "handlebars");
    
    app.use(logger("dev"));
    app.use(express.static(path.join(settings.root, "wwwroot")));
    app.use(errorHandler);

    function errorHandler(err, req, res, next) {
        res.status(500);
        res.json( { error: err });
    }

    var modules = glob.sync(settings.root + "/app/modules/controllers/**/*Controller.js");
    modules.forEach(function (module) {
		require(module)(app);
	});
};