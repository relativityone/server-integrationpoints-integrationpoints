echo "Creating connection to Azure Function"

def post = new URL("https://relativitysyncdashboards.azurewebsites.net/api/Function").openConnection();
def body = '{}'
post.setDoOutput(true)
post.setRequestMethod("POST")
post.setRequestProperty("x-functions-key", "key")
post.getOutputStream().write(body)
def response = post.getResponseCode();

println(response);