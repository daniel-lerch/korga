FROM node:14 as webapp
WORKDIR /app

# Copy package definition and restore node modules as distinct layers
COPY webapp/package.json webapp/package-lock.json ./
RUN npm install

COPY webapp ./
RUN npm run build
