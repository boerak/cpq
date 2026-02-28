FROM gorules/editor:latest AS editor-source
FROM nginx:alpine

# Copy editor SPA from gorules/editor image
COPY --from=editor-source /app/static /usr/share/nginx/html/editor

# Copy custom nginx config
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copy rules browser page
COPY rules-browser.html /usr/share/nginx/html/index.html

# Copy entrypoint that fixes permissions
COPY entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

EXPOSE 3000
ENTRYPOINT ["/entrypoint.sh"]
CMD ["nginx", "-g", "daemon off;"]
