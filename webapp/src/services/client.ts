const baseUrl =
  process.env.VUE_APP_API_URL ?? window.resourceBasePath.slice(0, -1);
const getInfo: RequestInit = {
  credentials: "include",
};
const postInfo: RequestInit = {
  method: "POST",
  credentials: "include",
};
const deleteInfo: RequestInit = {
  method: "DELETE",
  credentials: "include",
  headers: {
    "Content-Type": "application/json",
  },
};

export default {
  async get<T>(path: string) {
    const response = await fetch(baseUrl + path, getInfo);
    if (response.ok === false) {
      if (response.status === 401) {
        const responseData = await response.json();
        window.location.href = responseData.openIdConnectRedirectUrl;
      }
      throw new Error("Unexpected status code " + response.status);
    }
    return (await response.json()) as T;
  },
  async getResponse(path: string) {
    const response = await fetch(baseUrl + path, getInfo);
    if (response.ok === false) {
      if (response.status === 401) {
        const responseData = await response.json();
        window.location.href = responseData.openIdConnectRedirectUrl;
      }
      throw new Error("Unexpected status code " + response.status);
    }
    return response;
  },
  async post(path: string, data: object) {
    const response = await fetch(baseUrl + path, {
      ...postInfo,
      body: JSON.stringify(data),
    });
    if (response.ok === true) {
      return true;
    } else if (response.status === 409) {
      return false;
    } else {
      throw new Error("Unexpected status code " + response.status);
    }
  },
  async delete(path: string) {
    const response = await fetch(baseUrl + path, deleteInfo);
    if (response.ok === false) {
      throw new Error("Unexpected status code " + response.status);
    }
  },
};
