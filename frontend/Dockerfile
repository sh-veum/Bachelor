# # Modified code from https://www.youtube.com/watch?v=N9hRhcHpg1c
# # node image
# FROM node:16.3.0-alpine3.13 as build-stage
# # the working directory
# WORKDIR /app
# # copy the working directory in the container
# COPY package*.json ./
# # installing project dependecies 
# RUN  npm install
# COPY . .
# RUN npm run build
# FROM nginx:stable-alpine as production-stage
# COPY --from=build-stage /app/dist /usr/share/nginx/html
# COPY ./nginx/default.conf /etc/nginx/conf.d/default.conf
# EXPOSE 80
# CMD ["nginx", "-g", "daemon off;"]

# # docker-compose up # to start the compilation process
# # to install npm package run
# # docker-compose exec web npm i bootstrap

# Use a standard node base image
FROM node:18

# Set the working directory in the container
WORKDIR /app

# Copy package.json and package-lock.json to work directory
COPY package*.json ./

# Install project dependencies, try to ignore optional dependencies error
RUN npm install --force

# Copy the rest of the code
COPY . .

# Expose port 8080 for the dev server
EXPOSE 8080

# Command to run when the container starts
CMD ["npm", "run", "dev"]
