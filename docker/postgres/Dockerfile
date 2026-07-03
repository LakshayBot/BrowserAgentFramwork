FROM postgres:16-alpine

COPY docker/postgres/init.sql /docker-entrypoint-initdb.d/

ENV POSTGRES_DB=browser_agent
ENV POSTGRES_USER=browser_agent
ENV POSTGRES_PASSWORD=browser_agent

EXPOSE 5432
