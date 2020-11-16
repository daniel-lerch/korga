const baseUrl = process.env.VUE_APP_API_URL ?? window.resourceBasePath.slice(0, -1)

async function sendAndReceive<T> (method: string, query: string, body: object): Promise<T> {
  const response = await fetch(baseUrl + query, {
    method: method,
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(body)
  })
  if (response.ok === false) {
    throw new Error('Unexpected status code ' + response.status)
  }
  return await response.json() as T
}

export async function get<T> (query: string): Promise<T> {
  const response = await fetch(baseUrl + query)
  if (response.ok === false) {
    throw new Error('Unexpected status code ' + response.status)
  }
  return await response.json() as T
}

export function post<T> (query: string, body: object): Promise<T> {
  return sendAndReceive('POST', query, body)
}

export function put<T> (query: string, body: object): Promise<T> {
  return sendAndReceive('PUT', query, body)
}
