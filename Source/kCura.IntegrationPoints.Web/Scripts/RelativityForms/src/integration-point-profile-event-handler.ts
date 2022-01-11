import { EventNames } from "./types/eventNames";
import { EventHandlers } from "./types/eventHandlers";
import { IConvenienceApi } from "./types/convenienceApi";
import { test } from "./event-handlers/test";

export default function(eventNames: typeof EventNames, convenienceApi: IConvenienceApi) {
    const eventHandlers: EventHandlers = {
        [eventNames.PAGE_LOAD_COMPLETE]: test(convenienceApi),
    };

    return eventHandlers;
};