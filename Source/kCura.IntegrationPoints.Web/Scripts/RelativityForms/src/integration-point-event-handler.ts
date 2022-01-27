import { createConsole } from "./event-handlers/createConsole";
import { EventNames } from "./types/eventNames";
import { IConvenienceApi } from "./types/convenienceApi";

export default function (eventNames: typeof EventNames, convenienceApi: IConvenienceApi) {

    var eventHandlers = {};
    eventHandlers[eventNames.PAGE_LOAD_COMPLETE] = function () {
        console.log("Page load complete!");
    }

    eventHandlers[eventNames.CREATE_CONSOLE] = createConsole(convenienceApi);

    eventHandlers[eventNames.UPDATE_CONSOLE] = createConsole(convenienceApi);

    return eventHandlers;
};