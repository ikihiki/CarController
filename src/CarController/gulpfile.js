/// <binding AfterBuild='copy' />
/*
This file in the main entry point for defining Gulp tasks and using Gulp plugins.
Click here to learn more. http://go.microsoft.com/fwlink/?LinkId=518007
*/

var gulp = require('gulp');
var ts = require('gulp-typescript');
var bower = require('gulp-bower');

gulp.task('copy', function () {

    bower().pipe(gulp.dest('Client/lib/'))

    gulp.src("Client/Scripts/*.ts")
        .pipe(ts())
    .pipe(gulp.dest("wwwroot/Scripts"));

    gulp.src("Client/lib/angular/*.js")
    .pipe(gulp.dest("wwwroot/lib/angular"));

    gulp.src("Client/lib/angular-animate/*.js")
.pipe(gulp.dest("wwwroot/lib/angular-animate"));


    gulp.src("Client/lib/angular-signalr-hub/*.js")
    .pipe(gulp.dest("wwwroot/lib/angular-signalr-hub"));

    gulp.src("Client/lib/angular-ui-validate/dist/*.js")
.pipe(gulp.dest("wwwroot/lib/angular-ui-validate"));


    gulp.src("Client/lib/jquery/dist/*.js")
    .pipe(gulp.dest("wwwroot/lib/jquery/"));

    gulp.src("Client/lib/signalr/*.js")
    .pipe(gulp.dest("wwwroot/lib/signalr"));

    gulp.src("Client/lib/bootstrap/dist/**")
    .pipe(gulp.dest("wwwroot/lib/bootstrap"));

    gulp.src("Client/*.html")
    .pipe(gulp.dest("wwwroot"))

    gulp.src("Client/*.css")
.pipe(gulp.dest("wwwroot"))

});
