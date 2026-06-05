import axios from "axios";
import {
  ListToolsRequestSchema,
  CallToolRequestSchema
} from "@modelcontextprotocol/sdk/types.js";

export function registerLibraryTools(server, baseUrl) {
  const api = axios.create({
    baseURL: baseUrl
  });

  server.setRequestHandler(
    ListToolsRequestSchema,
    async () => ({
      tools: [
        {
          name: "get_books",
          description:
            "Get all books in the library with their availability status.",
          inputSchema: {
            type: "object",
            properties: {},
            required: []
          }
        },
        {
          name: "get_book",
          description: "Get details of a specific book by its ID.",
          inputSchema: {
            type: "object",
            properties: {
              id: {
                type: "number",
                description: "The book ID"
              }
            },
            required: ["id"]
          }
        },
        {
          name: "reserve_book",
          description:
            "Reserve a book by ID for a specific person.",
          inputSchema: {
            type: "object",
            properties: {
              id: {
                type: "number",
                description: "Book ID"
              },
              reservedBy: {
                type: "string",
                description: "Person reserving the book"
              }
            },
            required: ["id", "reservedBy"]
          }
        },
        {
          name: "return_book",
          description:
            "Return a previously reserved book by ID.",
          inputSchema: {
            type: "object",
            properties: {
              id: {
                type: "number",
                description: "Book ID"
              }
            },
            required: ["id"]
          }
        }
      ]
    })
  );

  server.setRequestHandler(
    CallToolRequestSchema,
    async request => {
      const { name, arguments: args } = request.params;

      try {
        switch (name) {
          case "get_books": {
            const response = await api.get("/books");

            return {
              content: [
                {
                  type: "text",
                  text: JSON.stringify(response.data, null, 2)
                }
              ]
            };
          }

          case "get_book": {
            try {
              const response = await api.get(
                `/books/${args.id}`
              );

              return {
                content: [
                  {
                    type: "text",
                    text: JSON.stringify(
                      response.data,
                      null,
                      2
                    )
                  }
                ]
              };
            } catch (error) {
              if (error.response?.status === 404) {
                return {
                  content: [
                    {
                      type: "text",
                      text: `Book with ID ${args.id} not found.`
                    }
                  ]
                };
              }

              throw error;
            }
          }

          case "reserve_book": {
            const response = await api.post(
              `/books/${args.id}/reserve`,
              {
                reservedBy: args.reservedBy
              }
            );

            return {
              content: [
                {
                  type: "text",
                  text:
                    typeof response.data === "string"
                      ? response.data
                      : JSON.stringify(
                          response.data,
                          null,
                          2
                        )
                }
              ]
            };
          }

          case "return_book": {
            const response = await api.post(
              `/books/${args.id}/return`
            );

            return {
              content: [
                {
                  type: "text",
                  text:
                    typeof response.data === "string"
                      ? response.data
                      : JSON.stringify(
                          response.data,
                          null,
                          2
                        )
                }
              ]
            };
          }

          default:
            throw new Error(`Unknown tool: ${name}`);
        }
      } catch (error) {
        return {
          isError: true,
          content: [
            {
              type: "text",
              text: error.message
            }
          ]
        };
      }
    }
  );
}