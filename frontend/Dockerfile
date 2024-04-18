# Modified code from https://www.youtube.com/watch?v=N9hRhcHpg1c
# Github: https://github.com/vongkeo/devla_flutter_app/blob/main/docker_vue_nginx
# Build frontend application
# Use the node image from official Docker Hub
FROM node:16.10.0-alpine3.13 as build-stage

# set the working directory
WORKDIR /app

# Copy the working directory in the container
COPY package*.json ./

# Install the project dependecies
RUN npm install 

# Copy the rest of the project files to the container
COPY . .

# Declare ARGs to receive build-time variables
ARG VITE_VUE_APP_API_URL
ARG VITE_VUE_APP_API_WEBSOCKET_URL

# Set the ARGs as environment variables so Vite can use them during build
ENV VITE_VUE_APP_API_URL=$VITE_VUE_APP_API_URL
ENV VITE_VUE_APP_API_WEBSOCKET_URL=$VITE_VUE_APP_API_WEBSOCKET_URL

#Build the Vue.js application to the production mode to dist folder
RUN npm run build

# use the lighweight Nignx image from the previus state to the nginx container
FROM nginx:stable-alpine as production-stage

# Copy the build application from the previos state to the Nginx container
# her we can see the path of the build application and the path where we want to copy it
COPY --from=build-stage /app/dist /usr/share/nginx/html

# Copy the nginx configuration file
# here should be the same name as the nginx configuration file in the project
COPY ./nginx/default.conf /etc/nginx/conf.d/default.conf

# Expose the port 80
EXPOSE 80

# start nginx to server the application
CMD ["nginx", "-g", "daemon off;"]