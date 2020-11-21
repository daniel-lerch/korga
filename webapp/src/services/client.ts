const baseUrl = process.env.VUE_APP_API_URL ?? window.resourceBasePath.slice(0, -1)

export async function get<T> (query: string): Promise<T> {
  const response = await fetch(baseUrl + query)
  if (response.ok === false) {
    throw new Error('Unexpected status code ' + response.status)
  }
  const responseText = await response.text()
  return JSON.parse(responseText, (key, value) => {
    if (key.endsWith('Time') && value !== null) {
      return new Date(value)
    } else {
      return value
    }
  }) as T
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
  const responseBody = await response.json() as T
  responseBody.conflict = response.status === 409
  return responseBody
}
