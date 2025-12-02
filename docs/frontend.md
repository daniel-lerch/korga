# Mailist frontend architecture

## Design decisions

### Vue vs. React vs. Angular

The decision for a frontend framework was made in September 2020.
In a quick research, I found Vue, React and Angular were the most popular frameworks at that time.
Comments stated that Angular was more comprehensive and harder to learn.
React had a colorful ecosystem, whereas Vue's was more concentrated around few official packages.
I prefered the latter approach which I was well fimiliar with from the .NET platform and took the decision for Vue.

### fetch API vs. Axios

For quite some time, Mailist's frontend used both, the fetch API and Axios.
When I started to implement cookie authentication however, it seemed better to use only one API.
In terms of its API I like Axios but its only browser backend using XMLHttpRequest internally has limitations in redirect handling, service workers, etc.
Therefore I built my own slim abstraction for the fetch API which is now consistently used in Mailist.
