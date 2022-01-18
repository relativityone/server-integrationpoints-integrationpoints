// Binds context to event handler scope
// Second argument provides any additional data that comes from event handler call
type IContext = Record<string, any>;
type IRestParameters = any[];
type IContextProvider = ((callback: (context: IContext, ...rest: IRestParameters) => void) => void);

export const contextProvider: IContextProvider = (f) => {
	return function (...rest: IRestParameters) {
		try {
			const context: IContext = this

            return f(context, ...rest)
		} catch (err) {
			console.error('Unable to run event handler')
			console.error(err)
		}
	}
}