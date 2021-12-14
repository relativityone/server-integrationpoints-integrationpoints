import { createConsole } from "./event-handlers/createConsole";
import { EventNames } from "./types/eventNames";
import { EventHandlers } from "./types/eventHandlers";
import { IConvenienceApi } from "./types/convenienceApi";

export default function(eventNames: typeof EventNames, convenienceApi: IConvenienceApi) {
    const eventHandlers: EventHandlers = {
        [eventNames.CREATE_CONSOLE]: createConsole(convenienceApi),
    };

    return eventHandlers;
};