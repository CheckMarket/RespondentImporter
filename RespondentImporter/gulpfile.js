/// <binding BeforeBuild='bundle' />
/*
This file in the main entry point for defining Gulp tasks and using Gulp plugins.
Click here to learn more. http://go.microsoft.com/fwlink/?LinkId=518007
*/
var paths = {
    app: "./wwwroot/assets/js/app/",
    dist: "./wwwroot/assets/js/dist/"
};

var gulp = require('gulp'),
    concat = require("gulp-concat"),
    rename = require("gulp-rename"),
    uglify = require("gulp-uglify");


gulp.task('bundle', function () {
    return gulp.src([paths.app + "ApiFactory.js", paths.app + "ImporterApp.js"])
               .pipe(concat("CM.Importer.App.js"))
                .pipe(gulp.dest(paths.dist))
                .pipe(rename("CM.Importer.App.min.js"))
                .pipe(uglify())
                .pipe(gulp.dest(paths.dist));
});