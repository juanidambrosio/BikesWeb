/// <binding BeforeBuild='build' Clean='clean' />
/*
This file in the main entry point for defining Gulp tasks and using Gulp plugins.
Click here to learn more. http://go.microsoft.com/fwlink/?LinkId=518007
*/

'use strict';

var gulp = require('gulp'),
    sass = require('gulp-sass'),
    rimraf = require("rimraf"),
    concat = require("gulp-concat"),
    uglify = require("gulp-uglify");


var paths = {
    webroot: "./wwwroot/content/",
    client: "./client/",
    bootstrap: "./node_modules/bootstrap/scss/"
};


gulp.task("clean", function (cb) {
    return rimraf(paths.webroot, cb);
});

gulp.task('build:css', function () {
    return gulp.src([paths.client + '**/*.scss', paths.bootstrap + 'bootstrap-flex.scss'])
        .pipe(sass({ outputStyle: 'compressed' }).on('error', sass.logError))
        .pipe(gulp.dest(paths.webroot));
});

gulp.task('sass:watch', function () {
    return gulp.watch('./src/**/*.scss', ['build:css']);
});

gulp.task("copy:fonts", function () {
    return gulp.src([paths.client + "**/fonts/**/*"])
        .pipe(gulp.dest(paths.webroot));
});
gulp.task("copy:images", function () {
    return gulp.src([paths.client + "**/images/**/*"])
        .pipe(gulp.dest(paths.webroot));
});

gulp.task("default", ["build:css", "copy:fonts", "copy:images"]);