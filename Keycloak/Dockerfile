FROM quay.io/keycloak/keycloak:22.0.5

COPY conaprole-realm-export.json /opt/keycloak/data/import/realm.json

EXPOSE 8080

CMD ["start", "--http-port=8080", "--hostname=container-conaprole-keycloak.delightfulbay-f2b42d90.brazilsouth.azurecontainerapps.io", "--hostname-strict=false", "--hostname-strict-https=false", "--proxy=edge", "--spi-x-frame-options-enabled=false", "--import-realm"]
