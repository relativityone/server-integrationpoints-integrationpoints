var registerSuite = intern.getInterface('object').registerSuite;
const { assert } = intern.getPlugin('chai');

var envDependantMessage = 'Do not add this test to repo, it\'s environment-dependant';
 

registerSuite('time-utils.js', {
        'isValidMilitaryTime':{
            'null': function () {
                assert.strictEqual(IP.timeUtil.isValidMilitaryTime(null), false , 'if not given any data, return false');
            },
            '1:43': function () {
                assert.strictEqual(IP.timeUtil.isValidMilitaryTime('1:43'), true , 'given correct data return true');
            },
            '16:43': function () {
                assert.strictEqual(IP.timeUtil.isValidMilitaryTime('16:43'), false , 'military time is correct when hours are between [1;12]');
            }
        },

        'isValidDate':{
            'null, YYYY/MM/DD': function () {
                assert.strictEqual(IP.timeUtil.isValidDate(null, 'YYYY/MM/DD'), false , 'if date is not present return false');
            },
            '2018/7/9, null': function () {
                assert.strictEqual(IP.timeUtil.isValidDate('2018/7/9',null), false , 'if format is not present return false');
            },
            '2018/07/09, YYYY/MM/DD': function () {
                assert.strictEqual(IP.timeUtil.isValidDate('2018/07/09','YYYY/MM/DD'), true , 'valid date should return true');
            },
            '2028/35/35, YYYY/MM/DD': function () {
                assert.strictEqual(IP.timeUtil.isValidDate('2028/35/35','YYYY/MM/DD'), false , 'month or day cannot be more than the amount of months/ days');
            },
            '18/12/25, YYYY/MM/DD': function () {
                assert.strictEqual(IP.timeUtil.isValidDate('18/12/25','YYYY/MM/DD'), false , 'year given is not in correct format');
            }
        },

        'isTodayOrInTheFuture':{
            'A': {

            },
            'today': function () {
                var curDate = new Date();
                var day = curDate.getDate();
                var month = curDate.getMonth()+1;
                var year = curDate.getFullYear();
                var dateString = month + '/' + day + '/' + year;
                assert.strictEqual(IP.timeUtil.isTodayOrInTheFuture(dateString, 'MM/DD/YYYY'), true , 'should be true because this date is today');
            },
            'future': function () {
                var curDate = new Date();
                var day = curDate.getDate();
                var month = curDate.getMonth()+1;
                var year = curDate.getFullYear();
                var dateString = month + '/' + day + '/' + (year+20);
                assert.strictEqual(IP.timeUtil.isTodayOrInTheFuture(dateString, 'MM/DD/YYYY'), true , 'should be true because this date is in the future');
            },
            'past': function () {
                var curDate = new Date();
                var day = curDate.getDate();
                var month = curDate.getMonth()+1;
                var year = curDate.getFullYear();
                var dateString = month + '/' + day + '/' + (year-10);
                assert.strictEqual(IP.timeUtil.isTodayOrInTheFuture(dateString, 'MM/DD/YYYY'), false , 'should be true because this date is ten years ago');
            }
        },

        'format24HourToMilitaryTime':{
            '8:33, h:mm': function () {
                assert.strictEqual(IP.timeUtil.format24HourToMilitaryTime('8:33', 'h:mm A'), "8:33 AM" , 'date must be converted with pm/am in the end');
            },
            '22:33, h:mm': function () {
                assert.strictEqual(IP.timeUtil.format24HourToMilitaryTime('22:33', 'h:mm A'), "10:33 PM" , 'date must be converted with pm/am in the end');
            },
            'null, h:mm': function () {
                assert.strictEqual(IP.timeUtil.format24HourToMilitaryTime(null, 'h:mm A'), "" , 'if date or format is null it should return ""');
            }
        },

        'formatMilitaryTimeTo24HourTime':{
            '8:33 AM, h:mm A': function () {
                assert.strictEqual(IP.timeUtil.formatMilitaryTimeTo24HourTime('8:33 AM', 'h:mm A'), "8:33" , 'date must be converted with pm/am in the end');
            },
            '3:33 PM, h:mm A': function () {
                assert.strictEqual(IP.timeUtil.formatMilitaryTimeTo24HourTime('3:33 PM', 'h:mm A'), "15:33" , 'date must be converted with pm/am in the end');
            },
            'null, h:mm A': function () {
                assert.strictEqual(IP.timeUtil.formatMilitaryTimeTo24HourTime(null, 'h:mm A'), "" , 'if date or format is null it should return ""');
            }
        },

        'formatDateTime':{
            '2018-07-05T06:58:44Z, MM/DD/YYYY h:mm A': function () {
                assert.strictEqual(IP.timeUtil.formatDateTime('2018-07-05T06:58:44Z', 'MM/DD/YYYY h:mm A'), '07/05/2018 8:58 AM' , 'datetime should be converted according to given format');
            },
            'null, MM/DD/YYYY h:mm A': function () {
                assert.strictEqual(IP.timeUtil.formatDateTime(null, 'MM/DD/YYYY h:mm A'), '' , 'if any of the parametrs is empty, function returns \'\'');
            },
        },

        'formatDate':{
            '12/22/2018, MM/DD/YYYY, YYYY/MM/DD': function () {
                assert.strictEqual(IP.timeUtil.formatDate('12/22/2018','MM/DD/YYYY','YYYY/MM/DD') , '2018/12/22' , 'datetime should be converted according to given format');
            },
            'null, MM/DD/YYYY, MM/DD/YYYY': function () {
                assert.strictEqual(IP.timeUtil.formatDate(null, 'MM/DD/YYYY, MM/DD/YYYY'), '' , 'if any of the parametrs is empty, function returns \'\'');
            }
        },

        'getCurrentUserDateFormat':{
            'no input': function () {
                assert.strictEqual(IP.timeUtil.getCurrentUserDateFormat(), 'MM/DD/YYYY' , envDependantMessage);
            }
        }
});