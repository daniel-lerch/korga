const baseUrl = process.env.VUE_APP_API_URL ?? window.resourceBasePath.slice(0, -1)

function jsonParse (text: string) {
  return JSON.parse(text, (key, value) => {
    if (key.endsWith('Time') && value !== null) {
      return new Date(value)
    } else {
      return value
    }
  })
}

export async function get<T> (query: string): Promise<T> {
  const response = await fetch(baseUrl + query)
  if (response.ok === false) {
    throw new Error('Unexpected status code ' + response.status)
  }
  const responseText = await response.text()
  return jsonParse(responseText) as T
}

export async function send<T extends {conflict: boolean}> (method: string, query: string, body?: object): Promise<T> {
  const init: RequestInit = { method: method }
  if (body !== undefined) {
    init.headers = {
      'Content-Type': 'application/json'
    }
    init.body = JSON.stringify(body)
  }
  const response = await fetch(baseUrl + query, init)
  if (![200, 409].includes(response.status)) {
    throw new Error('Unexpected status code ' + response.status + ' ' + response.statusText)
  }
  const responseText = await response.text()
  const responseBody = jsonParse(responseText) as T
  responseBody.conflict = response.status === 409
  return responseBody
}
