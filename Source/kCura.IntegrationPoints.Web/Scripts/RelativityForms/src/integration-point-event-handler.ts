import { createConsole } from "./event-handlers/createConsole";
import { EventNames } from "./types/eventNames";
import { EventHandlers } from "./types/eventHandlers";
import { IConvenienceApi } from "./types/convenienceApi";

export default function (eventNames: typeof EventNames, convenienceApi: IConvenienceApi) {

    var eventHandlers = {};
    eventHandlers[eventNames.PAGE_LOAD_COMPLETE] = function () {
        console.log("Page load complete!");
    }

    eventHandlers[eventNames.CREATE_CONSOLE] = createConsole(convenienceApi);

    eventHandlers[eventNames.UPDATE_CONSOLE] = function () {
        console.log("Update console complete!");
    }

    // [eventNames.CREATE_CONSOLE] = createConsole(convenienceApi);

    //const eventHandlers: EventHandlers = {
    //    [eventNames.CREATE_CONSOLE]: createConsole(convenienceApi),
    //};

    //eventHandlers[eventNames.HYDRATE_LAYOUT_COMPLETE] = function () {
    //    console.log("in hydrate");
    //}();

    return eventHandlers;
};