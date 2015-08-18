/// <reference path="angular-sanitize-1.2.d.ts" />
var shouldBeString;
shouldBeString = $sanitizeService(shouldBeString);
shouldBeString = $linky(shouldBeString);
shouldBeString = $linky(shouldBeString, shouldBeString);
