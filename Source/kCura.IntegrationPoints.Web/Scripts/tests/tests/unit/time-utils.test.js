(function () {
const { suite, test } = intern.getInterface('tdd');
const { assert } = intern.getPlugin('chai');


suite('time-utils.js', () => {
    suite('isValidMilitaryTime', () => {
        test('null', () => {
            assert.strictEqual(IP.timeUtil.isValidMilitaryTime(null), false , 'if not given any data, return false');
        });
        test('1:43', () => {
            assert.strictEqual(IP.timeUtil.isValidMilitaryTime('1:43'), true , 'given correct data return true');
        });
        test('16:43', () => {
            assert.strictEqual(IP.timeUtil.isValidMilitaryTime('16:43'), false , 'military time is correct when hours are between [1;12]');
        });
    });

    suite('isValidDate', () => {
        test('null, YYYY/MM/DD', () => {
            assert.strictEqual(IP.timeUtil.isValidDate(null, 'YYYY/MM/DD'), false , 'if date is not present return false');
        });
        test('2018/7/9, null', () => {
            assert.strictEqual(IP.timeUtil.isValidDate('2018/7/9',null), false , 'if format is not present return false');
        });
        test('2018/07/09, YYYY/MM/DD', () => {
            assert.strictEqual(IP.timeUtil.isValidDate('2018/07/09','YYYY/MM/DD'), true , 'valid date should return true');
        });
        test('2028/35/35, YYYY/MM/DD', () => {
            assert.strictEqual(IP.timeUtil.isValidDate('2028/35/35','YYYY/MM/DD'), false , 'month or day cannot be more than the amount of months/ days');
        });
        test('18/12/25, YYYY/MM/DD', () => {
            assert.strictEqual(IP.timeUtil.isValidDate('18/12/25','YYYY/MM/DD'), false , 'year given is not in correct format');
        });
    });

    suite('isTodayOrInTheFuture', () => {
        const curDate = new Date();
        const day = curDate.getDate();
        const month = curDate.getMonth()+1;
        const year = curDate.getFullYear();
        function formatDateString(amount) {
            return month + '/' + day + '/' + (year + amount);
        }

        test('today', () => { 
            const dateString = formatDateString(0);
            assert.strictEqual(IP.timeUtil.isTodayOrInTheFuture(dateString, 'MM/DD/YYYY'), true , 'should be true because this date is today');
        });
        test('future', () => { 
            const dateString = formatDateString(20);
            assert.strictEqual(IP.timeUtil.isTodayOrInTheFuture(dateString, 'MM/DD/YYYY'), true , 'should be true because this date is in the future');
        });
        test('past', () => { 
            const dateString = formatDateString(-10);
            assert.strictEqual(IP.timeUtil.isTodayOrInTheFuture(dateString, 'MM/DD/YYYY'), false , 'should be false because this date is ten years ago');
        });
    });

    suite('format24HourToMilitaryTime', () => {
        test('8:33, h:mm', () => {
            assert.strictEqual(IP.timeUtil.format24HourToMilitaryTime('8:33', 'h:mm A'), "8:33 AM" , 'date must be converted with pm/am in the end');
        });
        test('22:33, h:mm', () => {
            assert.strictEqual(IP.timeUtil.format24HourToMilitaryTime('22:33', 'h:mm A'), "10:33 PM" , 'date must be converted with pm/am in the end');
        });
        test('null, h:mm', () => {
            assert.strictEqual(IP.timeUtil.format24HourToMilitaryTime(null, 'h:mm A'), "" , 'if date or format is null it should return ""');
        });
    });

    suite('formatMilitaryTimeTo24HourTime', () => {
        test('8:33 AM, h:mm A', () => {
            assert.strictEqual(IP.timeUtil.formatMilitaryTimeTo24HourTime('8:33 AM', 'h:mm A'), "8:33" , 'date must be converted with pm/am in the end');
        });
        test('3:33 PM, h:mm A', () => {
            assert.strictEqual(IP.timeUtil.formatMilitaryTimeTo24HourTime('3:33 PM', 'h:mm A'), "15:33" , 'date must be converted with pm/am in the end');
        });
        test('null, h:mm A', () => {
            assert.strictEqual(IP.timeUtil.formatMilitaryTimeTo24HourTime(null, 'h:mm A'), "" , 'if date or format is null it should return ""');
        });
    });

    suite('formatDateTime', () => {
        test('2018-07-05T06:58:44Z, MM/DD/YYYY h:mm A', () => {
            assert.strictEqual(
                IP.timeUtil.formatDateTime('2018-07-05T06:58:44Z', 'MM/DD/YYYY h:mm A'),
                '07/05/2018 8:58 AM',
                'datetime should be converted according to given format'
            );
        });
        test('null, MM/DD/YYYY h:mm A', () => {
            assert.strictEqual(IP.timeUtil.formatDateTime(null, 'MM/DD/YYYY h:mm A'), '' , 'if any of the parametrs is empty, function returns \'\'');
        });
    });

    suite('formatDate', () => {
        test('12/22/2018, MM/DD/YYYY, YYYY/MM/DD', () => {
            assert.strictEqual(IP.timeUtil.formatDate('12/22/2018','MM/DD/YYYY','YYYY/MM/DD') , '2018/12/22' , 'datetime should be converted according to given format');
        });
        test('null, MM/DD/YYYY, MM/DD/YYYY', () => {
            assert.strictEqual(IP.timeUtil.formatDate(null, 'MM/DD/YYYY, MM/DD/YYYY'), '' , 'if any of the parametrs is empty, function returns \'\'');
        });
    });
})
})()