import { IConvenienceApi } from "../types/convenienceApi";

export function updeteJobHistoryTable(convenienceApi: IConvenienceApi, previousPage: string, workspaceId: number, artifactTypeID: number, viewId: number, integrationPointId: number, fieldId: number) {
    getListData(convenienceApi, workspaceId, artifactTypeID, viewId, integrationPointId, fieldId).then(res => {
        let newData = [];
        res.Objects.forEach(el => {
            newData.push(convertToJobHistory(el.Values));
        })

        let table = document.getElementById(fieldId.toString());
        let currentPage = convenienceApi.utilities.getRelativityPageBaseWindow().location.href;
        if (currentPage === previousPage) {
            if (checkIfArrayDataShouldBeUpdated(table["data"], newData)) {
                table["data"] = newData;
            }
            try {
                setTimeout(updeteJobHistoryTable, 5000, convenienceApi, currentPage, workspaceId, artifactTypeID, viewId, integrationPointId, fieldId);
            } catch (err) {
                console.log("Error occured while updating job history table data, will try once again in 5 secs", err)
                setTimeout(updeteJobHistoryTable, 5000, convenienceApi, currentPage, workspaceId, artifactTypeID, viewId, integrationPointId, fieldId);
            }
        } else {
        }
    })
}

async function getListData(convenienceApi: IConvenienceApi, workspaceId: number, artifactTypeID: number, viewId: number, integrationPointId: number, fieldId: number) {
    try {
        let sortsCondition = getSortParametersIfExists(fieldId);
        let filterCondition = getFilterParametersIfExists(fieldId);

        let table: any = document.getElementById(fieldId.toString());
        let tableState = table.getState();

        var url = `Relativity.Objects/workspace/${workspaceId}/object/queryslim`;
        var payload = {
            request: {
                objectType: { artifactTypeID: artifactTypeID },
                fields: [{ Name: "Job ID" }, { Name: "Start Time (UTC)" }, { Name: "Artifact ID" }, { Name: "Name" }, { Name: "Job Type" }, { Name: "Job Status" }, { Name: "Destination Workspace" }, { Name: "Items Transferred" }, { Name: "Total Items" }, { Name: "Items with Errors" }, { Name: "System Created By" }],
                condition: `('${fieldId}' INTERSECTS MULTIOBJECT [${integrationPointId}]) `,
                sorts: sortsCondition,
                rowCondition: filterCondition,
                executingViewId: viewId,
                convertNumberFieldValuesToString: true
            },
            start: tableState._startIndex,
            length: tableState._pageSize
        };
        var promise = convenienceApi.relativityHttpClient.keplerPost(url, payload);
        return promise.then(function (info) {
            return info;
        });
    } catch (err) {
        console.log(err)
    }
}

function getSortParametersIfExists(tableId: number): Array<any> {
    let table: any = document.getElementById(tableId.toString());
    let sorts: Array<any> = table._getSorts();
    if (sorts.length > 0) {
        return [{
            Direction: (sorts[0]._direction === "asc") ? "Ascending" : "Descending",
            FieldIdentifier: {
                Name: sorts[0]._column
            }
        }]
    }
    return [];
}

function getFilterParametersIfExists(tableId: number): string {
    let table: any = document.getElementById(tableId.toString());
    let filters: Array<any> = table._getFilters();

    filters = filters.filter((el => {
        return el._condition !== null
    }));
    if (filters.length === 0) {
        return "";
    }

    let stringFilter = ['Job ID', 'Name', 'Destination Workspace', 'System Created By'];
    let numberFilter = ['Artifact ID', 'Items Transferred', 'Total Items', 'Items with Errors'];
    let choiceFilter = ['Job Type', 'Job Status'];

    let conditions = [];
    filters.forEach(el => {
        if (stringFilter.includes(el._field)) {
            conditions.push(getConditionForString(el._field, el._condition._value))
        } else if (numberFilter.includes(el._field)) {
            conditions.push(GetConditionForNumber(el._field, el._condition._operator, el._condition._value))
        } else if (choiceFilter.includes(el._field)) {
            conditions.push(GetConditionForChoice(el._field, el._condition))
        } else {
            conditions.push(GetConditionForTime(el._field, el._condition[0]._value, el._condition[0]._operator))
        }
    })

    let condition = "(" + conditions.join(" AND ") + ")";
    return condition;
}

function getConditionForString(field: string, condition: string) {
    return `('${field}' LIKE ['${condition}'])`;
}

function GetConditionForNumber(field: string, operatorString: string, value: string) {
    let operators = {
        "is": "==",
        "is not": "!=",
        "is less than": "<",
        "is less than or equal to": "<=",
        "is greater than": ">",
        "is greater than or equal to": ">="
    }

    let operator = operators[operatorString];
    return `('${field}' ${operator} ${value})`;
}

function GetConditionForChoice(field: string, condition: Array<any>) {
    let conditionString = "(";
    let multipleCondiions = condition.length > 1 ? " OR " : "";
    condition.forEach(el => {
        if (el._operator === 'is not set') {
            conditionString += `(NOT '${field}' ISSET) ${multipleCondiions}`;
        }
        if (el._operator === "any of these") {

            let listOfChoices = ""
            el._value.forEach((val, i) => {
                let shouldHaveOR = (i === 0) ? "" : " OR ";
                listOfChoices += `${shouldHaveOR}('${field}' == CHOICE ${val})`;
            })
            conditionString += "(" + listOfChoices + ")";

        }
    })
    conditionString += ")";
    return conditionString;
}

function GetConditionForTime(field: string, values: Array<string>, operator: string) {
    values.forEach((el, index) => {
        if (el) {
            let date = new Date(el);
            date.setHours(date.getHours() - date.getTimezoneOffset() / 60);
            values[index] = date.toISOString();
        }
    })

    switch (operator) {
        case "between":
            return `('${field}' >= ${values[0].slice(0, -2) + "Z"} AND '${field}' <= ${values[1].slice(0, -2) + "Z"} )`;
        case "is":
            let date = new Date(values[0]);
            if (date.getTime() % 86400000 === 0) {
                let timeFrom = values[0];
                date.setHours(date.getHours() + 23);
                date.setMinutes(59);
                let timeTo = date.toISOString();
                return `('${field}' >= ${timeFrom.slice(0, -2) + "Z"} AND '${field}' <= ${timeTo.slice(0, -2) + "Z"} AND '${field}' ISSET)`;
            }
            return `('${field}' == ${values[0].slice(0, -2) + "Z"} )`;
        case 'is not set':
            return `(NOT '${field}' ISSET)`;
        default:
            let op = "";
            if (operator.includes("less")) {
                op = "<";
                if (operator.includes("equal")) {
                    op += "=";
                }
                return `('${field}' ${op} ${values[1].slice(0, -2) + "Z"} )`;
            } else if (operator.includes("greater ")) {
                op = ">";
                if (operator.includes("equal")) {
                    op += "=";
                }
                return `('${field}' ${op} ${values[0].slice(0, -2) + "Z"} )`;
            }
    }
}

function convertToJobHistory(el: Object) {
    let jobHistory = {
        "Job ID": el[0],
        "Start Time (UTC)": el[1],
        "Artifact ID": el[2],
        "Name": el[3],
        "Job Type": el[4],
        "Job Status": el[5],
        "Destination Workspace": el[6],
        "Items Transferred": el[7],
        "Total Items": el[8],
        "Items with Errors": el[9],
        "System Created By": el[10],
        "ArtifactID": el[11]
    }
    return jobHistory;
}

function checkIfObjectAreTheSame(row1: object, row2: object): boolean {
    let areSame = Object.keys(row1).every(key => {
        if ((row1[key] === null || row2[key] === null)) {
            if (row1[key] !== row2[key]) {
                return false;
            }
        } else if (typeof row1[key] === 'object') {
            if (row1[key].Name !== row2[key].Name) {
                return false;
            }
        } else {
            if (row1[key] !== row2[key]) {
                return false;
            }
        } return true;
    })
    return areSame;
}

function checkIfArrayDataShouldBeUpdated(currentData: Array<object>, pulledData: Array<object>): boolean {
    try {
        if (currentData.length !== pulledData.length) {
            return true;
        } else {
            let areSame = pulledData.every((element, index) => checkIfObjectAreTheSame(element, currentData[index]));
            return !areSame;
        }
        
    } catch (err){
        console.log(err)
    }
}