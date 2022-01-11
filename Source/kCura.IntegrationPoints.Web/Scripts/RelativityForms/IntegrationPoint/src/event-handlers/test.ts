import { contextProvider } from "../helpers/contextProvider";
import { IConvenienceApi } from "../types/convenienceApi";

export function test(convenienceApi: IConvenienceApi): void {
    return contextProvider((ctx) => {
        var consoleApi = convenienceApi.console;
        var integrationPointId = ctx.artifactId;
        var workspaceId = ctx.workspaceId;

        console.log("Integration Point Profile: Hello from Liquid Forms!");
        console.log(consoleApi, integrationPointId, workspaceId);
    })
}