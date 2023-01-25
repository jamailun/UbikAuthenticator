using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text;

namespace UbikMmo.Authenticator;
public class ExtractJson : IModelBinder {
	public async Task BindModelAsync(ModelBindingContext bindingContext) {
		string json;
		using(var reader = new StreamReader(bindingContext.ActionContext.HttpContext.Request.Body, Encoding.UTF8))
			json = await reader.ReadToEndAsync();

		bindingContext.Result = ModelBindingResult.Success(json);
	}
}
