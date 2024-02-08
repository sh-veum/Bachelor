# Modified code from https://www.youtube.com/watch?v=N9hRhcHpg1c
# node image
FROM node:16.3.0-alpine3.13 as build-stage
# the working directory
WORKDIR /app
# copy the working directory in the container
COPY package*.json ./
# installing project dependecies 
RUN  npm install
COPY . .
RUN npm run build
FROM nginx:stable-alpine as production-stage
COPY --from=build-stage /app/dist /usr/share/nginx/html
COPY ./nginx/default.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]

# docker-compose up # to start the compilation process
# to install npm package run
# docker-compose exec web npm i bootstrap