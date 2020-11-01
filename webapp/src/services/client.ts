export async function get<T> (query: string): Promise<T> {
  const response = await fetch('http://localhost:50805' + query)
  if (response.ok === false) {
    throw new Error('Unexpected status code ' + response.status)
  }
  return await response.json() as T
}

export async function post<T> (query: string, body: object): Promise<T> {
  const response = await fetch('http://localhost:50805' + query, {
    method: 'POST',
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
