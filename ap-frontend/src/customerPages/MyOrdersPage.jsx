import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import PopUpDialog from '../components/PopUpDialog'
import HandleExpiredCookies from '../components/HandleExpiredCookies'
import GetUserRole from '../components/GetUserRole'

const API_URL = import.meta.env.VITE_API_URL

function MyOrdersPage() {

  const [orders, setOrders] = useState([])
  const [isLoading, setIsLoading] = useState(true)
  const [isPopUpOpen, setIsPopUpOpen] = useState(false)
  const [popUpTitle, setPopUpTitle] = useState('')
  const [popUpMessage, setPopUpMessage] = useState('')

  const navigate = useNavigate()

  useEffect(() => {
    loadOrders()
  }, [])

  async function loadOrders() {
    const userRole = await GetUserRole()

    if (!userRole) {
      setPopUpTitle('Login Required')
      setPopUpMessage('Please log in to view your orders.')
      setIsPopUpOpen(true)
      setIsLoading(false)
      return
    }

    const response = await fetch(`${API_URL}/api/Orders`, {
      credentials: 'include',
    })

    if (HandleExpiredCookies(response)) {
      setIsLoading(false)
      return
    }

    if (!response.ok) {
      setPopUpTitle('Error')
      setPopUpMessage('Failed to load your orders.')
      setIsPopUpOpen(true)
      setIsLoading(false)
      return
    }

    const data = await response.json()

    setOrders(data)
    setIsLoading(false)
  }

  function handlePopUpClose() {
    setIsPopUpOpen(false)
  }

  function navigateToProduct(categoryName, productId) {
    const formattedCategoryName = categoryName.toLowerCase().replaceAll(' ', '-')
    navigate(`/products/${formattedCategoryName}/${productId}`)
  }

  if (isLoading) {
    return (
      <main className="min-h-screen bg-ap-tan px-6 pt-32 text-center text-ap-brown">
        <p className="text-sm uppercase tracking-widest md:text-base">
          Loading...
        </p>
      </main>
    )
  }

  return (
    <main className="min-h-screen bg-ap-tan px-6 py-28 text-ap-brown md:px-12 lg:px-20">
      <PopUpDialog
        isOpen={isPopUpOpen}
        onClose={handlePopUpClose}
        title={popUpTitle}
        message={popUpMessage}
      />

      <section className="mx-auto max-w-4xl md:max-w-5xl lg:max-w-6xl">
        <h1 className="font-['Tangerine'] text-5xl font-bold md:text-6xl lg:text-7xl">
          My Orders
        </h1>

        <p className="mt-2 text-sm uppercase tracking-widest md:text-base">
          Previous and current orders
        </p>

        <div className="mt-6 grid gap-6 md:mt-8 lg:mt-10">
          {orders.length > 0 ? (
            orders.map((order) => (
              <div
                key={order.orderId}
                className="overflow-hidden rounded-md border border-ap-brown bg-ap-pale transition duration-300 hover:shadow-md"
              >
                {/* Order information */}
                <div className="grid gap-5 p-5 md:grid-cols-2 md:p-6 lg:grid-cols-[1fr_2.5fr_1.2fr_1fr_1fr] lg:items-center lg:gap-6 lg:p-8">
                  <div>
                    <p className="text-xs font-medium uppercase tracking-widest md:text-sm">
                      Order ID
                    </p>

                    <p className="mt-2 text-sm md:text-base lg:text-lg">
                      #{order.orderId}
                    </p>
                  </div>

                  <div>
                    <p className="text-xs font-medium uppercase tracking-widest md:text-sm">
                      Order Date
                    </p>

                    <p className="mt-2 text-sm md:text-base">
                      {new Date(order.orderDate).toLocaleDateString('en-AU')}
                    </p>
                  </div>

                  <div>
                    <p className="text-xs font-medium uppercase tracking-widest md:text-sm">
                      Status
                    </p>

                    <p className="mt-2 text-sm uppercase tracking-widest md:text-base">
                      {order.status}
                    </p>
                  </div>

                  <div>
                    <p className="text-xs font-medium uppercase tracking-widest md:text-sm">
                      Total
                    </p>

                    <p className="mt-2 text-sm font-medium md:text-base lg:text-lg">
                      ${Number(order.totalPrice).toFixed(2)}
                    </p>
                  </div>
                </div>

                {/* Items in the order */}
                <div className="border-t border-ap-brown px-5 py-5 md:px-6 lg:px-8">
                  <p className="text-xs font-medium uppercase tracking-widest md:text-sm">
                    Items
                  </p>

                  <div className="mt-4 grid gap-3">
                    {order.orderItems.length > 0 ? (
                      order.orderItems.map((item) => (
                        <div
                          key={item.productId}
                          className="grid grid-cols-2 gap-3 rounded-md border border-ap-brown bg-white transition duration-300 hover:shadow-md hover:cursor-pointer p-4 md:grid-cols-4 md:items-center"
                          onClick={() => navigateToProduct(item.categoryName, item.productId)}
                        >
                          <div>
                            <img
                              src={item.imageUrl}
                              alt={item.productName}
                              className="aspect-square w-15 rounded-md object-cover md:w-20 lg:w-25"
                            />
                          </div>
                          
                          <div>
                            <p className="text-xs font-medium uppercase tracking-widest md:text-sm">
                              Product Name
                            </p>

                            <p className="mt-1 text-sm md:text-base">
                              #{item.productName}
                            </p>
                          </div>

                          <div>
                            <p className="text-xs font-medium uppercase tracking-widest md:text-sm">
                              Quantity
                            </p>

                            <p className="mt-1 text-sm md:text-base">
                              {item.quantity}
                            </p>
                          </div>

                          <div>
                            <p className="text-xs font-medium uppercase tracking-widest md:text-sm">
                              Price
                            </p>

                            <p className="mt-1 text-sm font-medium md:text-base">
                              ${Number(item.priceAtPurchase).toFixed(2)}
                            </p>
                          </div>
                        </div>
                      ))
                    ) : (
                      <p className="text-sm uppercase tracking-widest">
                        No items found for this order.
                      </p>
                    )}
                  </div>
                </div>
              </div>
            ))
          ) : (
            <div className="rounded-md border border-ap-brown bg-ap-pale p-8 text-center transition duration-300 hover:shadow-lg">
              <p className="text-sm uppercase tracking-widest md:text-base lg:text-lg">
                You have no orders yet.
              </p>
            </div>
          )}
        </div>
      </section>
    </main>
  )
}

export default MyOrdersPage