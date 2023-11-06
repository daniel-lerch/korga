export interface ProfileResponse {
    id: string;
    givenName: string;
    familyName: string;
    emailAddress: string;
}

export const useProfileStore = defineStore('profile', {
    state: () => ({
        profile: undefined as ProfileResponse | null | undefined,
    }),
    actions: {
        async init() {
            if (this.profile === undefined) {
                const { data } = await useFetch<ProfileResponse | null>('https://lerchen.net/korga/api/profile', { credentials: 'include' })
                this.profile = data.value
            }
        },
        async refresh() {
            this.profile = await $fetch<ProfileResponse | null>('https://lerchen.net/korga/api/profile', { credentials: 'include' })
        },
    },
})
