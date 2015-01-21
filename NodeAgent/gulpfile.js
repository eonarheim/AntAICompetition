/**
 * Created by Timothy on 1/19/2015.
 */

var jshint = require('gulp-jshint');
var gulp   = require('gulp');
var stylish = require('jshint-stylish');


gulp.task('lint', function() {
    return gulp.src('./*.js')
        .pipe(jshint())
        .pipe(jshint.reporter(stylish));
});

gulp.task('default', function() {
    gulp.run('lint');
});
