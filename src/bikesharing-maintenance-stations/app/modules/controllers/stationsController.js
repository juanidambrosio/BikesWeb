"use strict";
var settings = require("../../../config/settings");
var http = require("http");
var Q = require('Q');
var dateformat = require('dateformat');

module.exports = function (router) {

	function getStations(request){
		var query = request.from ? `?from=${request.from}` : "";
		var endpoint = `${settings.stationsApiBaseUrl}api/stations/tenant/1/${query}`;
		return getHttpRequest(endpoint);
	}

	function getUser(request){
		var endpoint = `${settings.accountsBaseUrl}User/Info/${request.id}`;
		return getHttpRequest(endpoint, {errorResolve: {
			name: '',
			image: null
		}});
	}

	function getHttpRequest(endpoint, options){
		var deferred = Q.defer();
		options = options || {};
		http.get(endpoint, (res) => {
			const statusCode = res.statusCode;
			const contentType = res.headers['content-type'];
			let error;
			if (statusCode !== 200) {
				error = new Error(`Error requesting ${endpoint}. Status code: ${statusCode}`);
			} else if (!/^application\/json/.test(contentType)) {
				error = new Error(`Invalid content-type. Expected application/json but received ${contentType}`);
			}
			if (error) {
				res.resume();
				deferred.reject(error);
				return;
			}

			res.setEncoding('utf8');
			let rawData = '';
			res.on('data', (chunk) => rawData += chunk);
			res.on('end', () => {
				try {
					let parsedData = JSON.parse(rawData);
					deferred.resolve({
						data: parsedData,
						headers: res.headers
					});
				} catch (e) {					
					deferred.reject(e);
				}
			});
		}).on('error', (e) => {
			if (options.errorResolve) {
				deferred.resolve({data: options.errorResolve});
			}
			else {
				deferred.reject(e);
			}
		});
		
    	return deferred.promise;
	}

	function mapStations(data){
		var mappedStations = [];
		data.forEach(function(station){
			var slots = station.slots;
			var occupied = station.occupied;
			
			mappedStations.push({
				id: station.id,
				name: station.name,
				freeSlots: slots - occupied,
				parkedBikes: occupied,
				total: slots
			});
		});
		return mappedStations;
	}

	router.get("/stations", (req, res) => {
		if(!req.query.id){
			res.redirect(`${settings.accountsBaseUrl}Account/Login`);
			return;
		}

		var stationsRequest = {
			id: 1,
			from: parseInt(req.query.from || 0),
			size: 20
		};

		var model = {
			headerModel: {
				navLinks: {
					dashboard: settings.navLinks.dashboard,
					issues: settings.navLinks.issues,
					subscription: settings.navLinks.subscription,
					rides: settings.navLinks.rides,
				}
			},
			pagination: {
				from: stationsRequest.from,
				currentDisplayPage: stationsRequest.from+1,
				prev: "?id="+req.query.id+"&from="+(stationsRequest.from > 0 ? stationsRequest.from - 1 : 0),
				next: "?id="+req.query.id+"&from="+(stationsRequest.from + 1),
			}
		};
		model.headerModel.navLinks.dashboard += `?id=${req.query.id}`;
		

		var stationsPromise = getStations(stationsRequest)
			.then(function(response){
				model.stations = mapStations(response.data);
				let totalPages = response.headers.total/stationsRequest.size;;
				if(totalPages > parseInt(totalPages)){
					totalPages++;
				}
				totalPages = parseInt(totalPages);
				model.pagination.totalPages = totalPages;
				model.pagination.totalItems = parseInt(response.headers.total);

			});

		var userPromise = getUser({id: req.query.id})
			.then(function(response){
				model.headerModel.user = {
					id: req.query.id,
					name: response.data.name,
					avatar: response.data.image,
					logoutAction: `${settings.accountsBaseUrl}Account/Logout`
				};
			});

		Q.all([stationsPromise, userPromise])
			.then(function(){
				res.render("stations", model);
			}, function(e){
				console.log(`Got error: ${e}`);
				res.render("error", model);
			});
	});
};